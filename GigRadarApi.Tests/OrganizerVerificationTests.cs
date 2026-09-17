using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test OrganizerVerificationService (GIGRADAR_APP_SPEC.md Bab 12–15, 22) — EF InMemory.
/// Fokus: profil + portofolio wajib sebelum submit, aturan satu PENDING,
/// resubmit setelah ditolak, keputusan Admin yang selalu mengubah status profil EO,
/// ownership portofolio, dan filter whitelist di daftar Verification Center.
/// </summary>
public class OrganizerVerificationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly OrganizerVerificationService _service;
    private readonly NotificationService _notificationService;

    // Id user seed: 1 = Admin.
    private const int AdminId = 1;
    private const int EoUserId = 20;

    public OrganizerVerificationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _notificationService = new NotificationService(_context);
        _service = new OrganizerVerificationService(_context, _notificationService);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private async Task<Organizer> SeedOrganizerAsync(int userId = EoUserId)
    {
        var organizer = await _service.GetOrCreateOwnOrganizerAsync(userId);
        // Lengkapi profil ke kondisi "siap submit" (deskripsi + lokasi wajib, Bab 13).
        organizer.Description = "Promotor gig indie Jakarta.";
        organizer.Location = "Jakarta";
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return organizer;
    }

    private async Task AddPortfolioAsync(int count = 1)
    {
        for (var i = 1; i <= count; i++)
        {
            var (success, message) = await _service.AddPortfolioItemAsync(EoUserId, new PortfolioItemRequest
            {
                EventName = $"Panggung Indie #{i}",
                Venue = "Bentara Budaya",
                Description = "Gig tahunan dengan 500 penonton.",
                DocumentationLink = $"https://docs.example.com/gig-{i}"
            });
            Assert.True(success, message);
        }
    }

    private static OrganizerVerificationRequest ValidRequest() => new()
    {
        Contact = "081234567890"
    };

    // ── Profil & portofolio ──────────────────────────────

    [Fact]
    public async Task UpdateProfile_EmptyOrganizationName_Rejected()
    {
        await SeedOrganizerAsync();

        var (success, message) = await _service.UpdateProfileAsync(
            EoUserId, "   ", "desc", "Jakarta", "0812", null, null, null);

        Assert.False(success);
        Assert.Contains("Nama organisasi", message);
    }

    [Fact]
    public async Task UpdateProfile_WhilePending_ResetsToNotSubmitted()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        var (success, _) = await _service.UpdateProfileAsync(
            EoUserId, "Nama Baru Promotor", "desc baru", "Bandung", "0813", null, null, 2020);

        Assert.True(success);
        var organizer = await _context.Organizers.AsNoTracking().SingleAsync(o => o.UserId == EoUserId);
        Assert.Equal("NOT_SUBMITTED", organizer.VerificationStatus);
        Assert.Equal("Nama Baru Promotor", organizer.OrganizationName);
    }

    [Fact]
    public async Task AddPortfolio_MissingEventName_Rejected()
    {
        await SeedOrganizerAsync();

        var (success, message) = await _service.AddPortfolioItemAsync(EoUserId, new PortfolioItemRequest
        {
            EventName = "  ",
            Description = "ada deskripsi"
        });

        Assert.False(success);
        Assert.Contains("Nama event", message);
    }

    [Fact]
    public async Task AddPortfolio_NoEvidence_Rejected()
    {
        await SeedOrganizerAsync();

        var (success, message) = await _service.AddPortfolioItemAsync(EoUserId, new PortfolioItemRequest
        {
            EventName = "Gig Tanpa Bukti"
        });

        Assert.False(success);
        Assert.Contains("bukti", message);
    }

    [Fact]
    public async Task DeletePortfolio_OwnedByOther_Fails()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var item = await _context.OrganizerPortfolioItems.AsNoTracking().FirstAsync();

        var (success, message) = await _service.DeletePortfolioItemAsync(999, item.PortfolioItemId);

        Assert.False(success);
        // User lain (tanpa profil organizer) tidak bisa menghapus portofolio milik orang lain.
        Assert.True(message.Contains("belum ada") || message.Contains("tidak ditemukan"), message);
        Assert.Single(await _context.OrganizerPortfolioItems.ToListAsync());
    }

    // ── Submit ───────────────────────────────────────────

    [Fact]
    public async Task Submit_ValidRequest_CreatesPendingWithPortfolioSnapshot()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync(count: 2);

        var (success, message, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.True(success, message);
        var verification = Assert.IsType<OrganizerVerification>(result);
        Assert.Equal("PENDING", verification.Status);
        Assert.Equal(2, verification.PortfolioCount);

        var organizer = await _context.Organizers.AsNoTracking().SingleAsync(o => o.UserId == EoUserId);
        Assert.Equal("PENDING", organizer.VerificationStatus);
        Assert.False(organizer.IsVerified);
    }

    [Fact]
    public async Task Submit_MissingDescription_Rejected()
    {
        await SeedOrganizerAsync();
        var organizer = await _context.Organizers.SingleAsync(o => o.UserId == EoUserId);
        organizer.Description = "";
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        await AddPortfolioAsync();

        var (success, message, _) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.False(success);
        Assert.Contains("deskripsi", message);
    }

    [Fact]
    public async Task Submit_InvalidContact_Rejected()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var request = ValidRequest();
        request.Contact = "123";

        var (success, message, _) = await _service.SubmitVerificationAsync(EoUserId, request);

        Assert.False(success);
        Assert.Contains("kontak", message);
    }

    [Fact]
    public async Task Submit_NoPortfolio_Rejected()
    {
        await SeedOrganizerAsync();

        var (success, message, _) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.False(success);
        Assert.Contains("portofolio", message);
    }

    [Fact]
    public async Task Submit_DuplicatePending_Rejected()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        var (success, message, _) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.False(success);
        Assert.Contains("sedang ditinjau", message);
        Assert.Single(await _context.OrganizerVerifications.ToListAsync());
    }

    [Fact]
    public async Task Submit_ResubmitAfterReject_Allowed()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, first) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        await _service.ReviewAsync(((OrganizerVerification)first!).VerificationId, AdminId,
            approve: false, reason: "Dokumentasi kurang");

        var (success, message, second) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.True(success, message);
        var organizerId = (await _context.Organizers.AsNoTracking()
            .SingleAsync(o => o.UserId == EoUserId)).OrganizerId;
        var pending = await _context.OrganizerVerifications
            .Where(v => v.OrganizerId == organizerId && v.Status == "PENDING")
            .ToListAsync();
        Assert.Single(pending);
    }

    // ── Review (Admin) ───────────────────────────────────

    [Fact]
    public async Task Review_Approve_SetsVerifiedAndJournalsDecision()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        var verificationId = ((OrganizerVerification)result!).VerificationId;

        var (success, message) = await _service.ReviewAsync(verificationId, AdminId, approve: true, reason: null);

        Assert.True(success, message);
        var verification = await _context.OrganizerVerifications.AsNoTracking()
            .SingleAsync(v => v.VerificationId == verificationId);
        Assert.Equal("VERIFIED", verification.Status);
        Assert.Equal(AdminId, verification.ReviewedByUserId);
        Assert.NotNull(verification.ReviewedAt);

        var organizer = await _context.Organizers.AsNoTracking().SingleAsync(o => o.UserId == EoUserId);
        Assert.Equal("VERIFIED", organizer.VerificationStatus);
        Assert.True(organizer.IsVerified);
    }

    [Fact]
    public async Task Review_Reject_RecordsReasonAndClearsVerified()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        var verificationId = ((OrganizerVerification)result!).VerificationId;

        var (success, message) = await _service.ReviewAsync(verificationId, AdminId,
            approve: false, reason: "Portofolio tidak dapat diverifikasi");

        Assert.True(success, message);
        var verification = await _context.OrganizerVerifications.AsNoTracking()
            .SingleAsync(v => v.VerificationId == verificationId);
        Assert.Equal("REJECTED", verification.Status);
        Assert.Equal("Portofolio tidak dapat diverifikasi", verification.RejectedReason);

        var organizer = await _context.Organizers.AsNoTracking().SingleAsync(o => o.UserId == EoUserId);
        Assert.Equal("REJECTED", organizer.VerificationStatus);
        Assert.False(organizer.IsVerified);
    }

    [Fact]
    public async Task Review_AlreadyDecided_Rejected()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        var verificationId = ((OrganizerVerification)result!).VerificationId;
        await _service.ReviewAsync(verificationId, AdminId, approve: true, reason: null);

        var (success, message) = await _service.ReviewAsync(verificationId, AdminId, approve: false, reason: "coba lagi");

        Assert.False(success);
        Assert.Contains("sudah diputuskan", message);
    }

    [Fact]
    public async Task Review_UnknownVerification_Fails()
    {
        var (success, message) = await _service.ReviewAsync(4242, AdminId, approve: true, reason: null);

        Assert.False(success);
        Assert.Contains("tidak ditemukan", message);
    }

    // ── Status & daftar (Verification Center) ────────────

    [Fact]
    public async Task GetMyStatus_ReflectsStateCanSubmitAndPortfolioCount()
    {
        await SeedOrganizerAsync();

        var before = AsJson(await _service.GetMyStatusAsync(EoUserId));
        Assert.Equal("NOT_SUBMITTED", before.GetProperty("verificationStatus").GetString());
        Assert.True(before.GetProperty("canSubmit").GetBoolean());
        Assert.Equal(0, before.GetProperty("portfolioCount").GetInt32());

        await AddPortfolioAsync();
        await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        var pending = AsJson(await _service.GetMyStatusAsync(EoUserId));
        Assert.Equal("PENDING", pending.GetProperty("verificationStatus").GetString());
        Assert.False(pending.GetProperty("canSubmit").GetBoolean());
        Assert.Equal(1, pending.GetProperty("portfolioCount").GetInt32());
    }

    [Fact]
    public async Task GetMyStatus_RejectedSubmission_ExposesReasonAndAllowsResubmit()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        var verificationId = ((OrganizerVerification)result!).VerificationId;
        await _service.ReviewAsync(verificationId, AdminId, approve: false, reason: "Organisasi tidak jelas");

        var status = AsJson(await _service.GetMyStatusAsync(EoUserId));
        Assert.Equal("REJECTED", status.GetProperty("verificationStatus").GetString());
        Assert.True(status.GetProperty("canSubmit").GetBoolean());
        Assert.Equal("Organisasi tidak jelas",
            status.GetProperty("latest").GetProperty("rejectedReason").GetString());
    }

    [Fact]
    public async Task GetMyStatus_UnknownUser_ReturnsNull()
    {
        Assert.Null(await _service.GetMyStatusAsync(999));
    }

    [Fact]
    public async Task GetList_StatusFilter_ReturnsMatchingOnly()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        var (_, _, result) = await _service.SubmitVerificationAsync(EoUserId, ValidRequest());
        var verificationId = ((OrganizerVerification)result!).VerificationId;

        var pending = await _service.GetListAsync("PENDING");
        Assert.Single(pending);

        await _service.ReviewAsync(verificationId, AdminId, approve: true, reason: null);

        Assert.Empty(await _service.GetListAsync("PENDING"));
        Assert.Single(await _service.GetListAsync("VERIFIED"));
        Assert.Single(await _service.GetListAsync(status: null));
    }

    [Fact]
    public async Task GetList_InvalidStatusFilter_ReturnsEmpty()
    {
        await SeedOrganizerAsync();
        await AddPortfolioAsync();
        await _service.SubmitVerificationAsync(EoUserId, ValidRequest());

        Assert.Empty(await _service.GetListAsync("HACKED"));
    }

    private static JsonElement AsJson(object? o) => JsonDocument.Parse(
        JsonSerializer.Serialize(o, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
        .RootElement;
}
