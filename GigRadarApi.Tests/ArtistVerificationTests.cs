using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test ArtistVerificationService (GIGRADAR_APP_SPEC.md Bab 7–8, 22) — memakai EF InMemory.
/// Fokus: validasi pengajuan, aturan satu PENDING, resubmit setelah ditolak,
/// keputusan Admin (approve/reject) yang selalu mengubah status profil artist,
/// dan filter whitelist di daftar Verification Center.
/// </summary>
public class ArtistVerificationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ArtistVerificationService _service;
    private readonly NotificationService _notificationService;

    // Id user seed: 1 = Admin.
    private const int AdminId = 1;
    private const int ArtistUserId = 10;

    public ArtistVerificationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _notificationService = new NotificationService(_context);
        _service = new ArtistVerificationService(_context, _notificationService);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private async Task<Artist> SeedArtistAsync(int userId = ArtistUserId)
    {
        var artist = new Artist { Name = "Hollow Men", Genre = "Shoegaze", City = "Jakarta", UserId = userId };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return artist;
    }

    private static ArtistVerificationRequest ValidRequest() => new()
    {
        LegalName = "Raka Pratama",
        Phone = "081234567890",
        SpotifyUrl = "https://open.spotify.com/artist/hollow-men",
        SupportingInfo = "Band aktif sejak 2024, sering tampil di Jakarta."
    };

    // ── Submit ───────────────────────────────────────────

    [Fact]
    public async Task GetArtist_IncludesSpotifyUrl()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var artist = new Artist
        {
            Name = "Band Murphy Radio",
            SpotifyUrl = "https://open.spotify.com/artist/2DDfSj2dukX0wA8Xe0pk1N?si=W5wcGBhASvuWT78I75xOcQ",
            Genre = "Indie Rock",
            City = "Jakarta"
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var controller = new ArtistsController(context);
        var result = await controller.GetArtist(artist.ArtistId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = ok.Value!;
        var spotifyUrl = payload.GetType().GetProperty("SpotifyUrl")?.GetValue(payload)?.ToString();

        Assert.Equal("https://open.spotify.com/artist/2DDfSj2dukX0wA8Xe0pk1N?si=W5wcGBhASvuWT78I75xOcQ", spotifyUrl);
    }

    [Fact]
    public async Task Submit_ValidRequest_CreatesPendingAndUpdatesArtist()
    {
        var artist = await SeedArtistAsync();

        var (success, message, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());

        Assert.True(success, message);
        var verification = Assert.IsType<ArtistVerification>(result);
        Assert.Equal("PENDING", verification.Status);

        var artistDb = await _context.Artists.AsNoTracking().SingleAsync(a => a.ArtistId == artist.ArtistId);
        Assert.Equal("PENDING", artistDb.VerificationStatus);
        Assert.False(artistDb.IsVerified);
    }

    [Fact]
    public async Task Submit_MissingLegalName_Rejected()
    {
        await SeedArtistAsync();
        var request = ValidRequest();
        request.LegalName = "   ";

        var (success, message, _) = await _service.SubmitAsync(ArtistUserId, request);

        Assert.False(success);
        Assert.Contains("Nama legal", message);
        Assert.Empty(await _context.ArtistVerifications.ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345678")]
    public async Task Submit_InvalidPhone_Rejected(string? phone)
    {
        await SeedArtistAsync();
        var request = ValidRequest();
        request.Phone = phone;

        var (success, message, _) = await _service.SubmitAsync(ArtistUserId, request);

        Assert.False(success);
        Assert.Contains("telepon", message);
    }

    [Fact]
    public async Task Submit_NoSupportingEvidence_Rejected()
    {
        await SeedArtistAsync();
        var request = ValidRequest();
        request.SpotifyUrl = "";
        request.SupportingInfo = " ";

        var (success, message, _) = await _service.SubmitAsync(ArtistUserId, request);

        Assert.False(success);
        Assert.Contains("Spotify", message);
    }

    [Fact]
    public async Task Submit_UserWithoutArtistProfile_Rejected()
    {
        var (success, message, _) = await _service.SubmitAsync(999, ValidRequest());

        Assert.False(success);
        Assert.Contains("Profil artist belum ada", message);
    }

    [Fact]
    public async Task Submit_DuplicatePending_Rejected()
    {
        await SeedArtistAsync();
        await _service.SubmitAsync(ArtistUserId, ValidRequest());

        var (success, message, _) = await _service.SubmitAsync(ArtistUserId, ValidRequest());

        Assert.False(success);
        Assert.Contains("sedang ditinjau", message);
        Assert.Single(await _context.ArtistVerifications.ToListAsync());
    }

    [Fact]
    public async Task Submit_ResubmitAfterReject_Allowed()
    {
        var artist = await SeedArtistAsync();
        var (_, _, first) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        await _service.ReviewAsync(((ArtistVerification)first!).VerificationId, AdminId, approve: false, reason: "Bukti kurang");

        var (success, message, second) = await _service.SubmitAsync(ArtistUserId, ValidRequest());

        Assert.True(success, message);
        var pending = await _context.ArtistVerifications
            .Where(v => v.ArtistId == artist.ArtistId && v.Status == "PENDING")
            .ToListAsync();
        Assert.Single(pending);
        Assert.Equal(2, await _context.ArtistVerifications.CountAsync(v => v.ArtistId == artist.ArtistId));
    }

    // ── Review (Admin) ───────────────────────────────────

    [Fact]
    public async Task Review_Approve_SetsVerifiedAndJournalsDecision()
    {
        var artist = await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;

        var (success, message) = await _service.ReviewAsync(verificationId, AdminId, approve: true, reason: null);

        Assert.True(success, message);
        var verification = await _context.ArtistVerifications.AsNoTracking()
            .SingleAsync(v => v.VerificationId == verificationId);
        Assert.Equal("VERIFIED", verification.Status);
        Assert.Equal(AdminId, verification.ReviewedByUserId);
        Assert.NotNull(verification.ReviewedAt);
        Assert.Equal("", verification.RejectedReason);

        var artistDb = await _context.Artists.AsNoTracking().SingleAsync(a => a.ArtistId == artist.ArtistId);
        Assert.Equal("VERIFIED", artistDb.VerificationStatus);
        Assert.True(artistDb.IsVerified);
    }

    [Fact]
    public async Task Review_Reject_RecordsReasonAndClearsVerified()
    {
        var artist = await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;

        var (success, message) = await _service.ReviewAsync(verificationId, AdminId, approve: false, reason: "Akun Spotify tidak cocok");

        Assert.True(success, message);
        var verification = await _context.ArtistVerifications.AsNoTracking()
            .SingleAsync(v => v.VerificationId == verificationId);
        Assert.Equal("REJECTED", verification.Status);
        Assert.Equal("Akun Spotify tidak cocok", verification.RejectedReason);

        var artistDb = await _context.Artists.AsNoTracking().SingleAsync(a => a.ArtistId == artist.ArtistId);
        Assert.Equal("REJECTED", artistDb.VerificationStatus);
        Assert.False(artistDb.IsVerified);
    }

    [Fact]
    public async Task Review_RejectWithoutReason_UsesDefaultReason()
    {
        await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;

        var (success, _) = await _service.ReviewAsync(verificationId, AdminId, approve: false, reason: "   ");

        Assert.True(success);
        var verification = await _context.ArtistVerifications.AsNoTracking()
            .SingleAsync(v => v.VerificationId == verificationId);
        Assert.Equal("Tidak memenuhi kriteria verifikasi", verification.RejectedReason);
    }

    [Fact]
    public async Task Review_AlreadyDecided_Rejected()
    {
        await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;
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
    public async Task GetMyStatus_ReflectsStateAndCanSubmit()
    {
        await SeedArtistAsync();

        var before = AsJson(await _service.GetMyStatusAsync(ArtistUserId));
        Assert.Equal("NOT_SUBMITTED", before.GetProperty("verificationStatus").GetString());
        Assert.True(before.GetProperty("canSubmit").GetBoolean());

        await _service.SubmitAsync(ArtistUserId, ValidRequest());

        var pending = AsJson(await _service.GetMyStatusAsync(ArtistUserId));
        Assert.Equal("PENDING", pending.GetProperty("verificationStatus").GetString());
        Assert.False(pending.GetProperty("canSubmit").GetBoolean());
        Assert.True(pending.TryGetProperty("latest", out var latest) && latest.ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task GetMyStatus_RejectedSubmission_ExposesReasonAndAllowsResubmit()
    {
        await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;
        await _service.ReviewAsync(verificationId, AdminId, approve: false, reason: "Portofolio kurang");

        var status = AsJson(await _service.GetMyStatusAsync(ArtistUserId));
        Assert.Equal("REJECTED", status.GetProperty("verificationStatus").GetString());
        Assert.True(status.GetProperty("canSubmit").GetBoolean());
        Assert.Equal("Portofolio kurang",
            status.GetProperty("latest").GetProperty("rejectedReason").GetString());
    }

    private static JsonElement AsJson(object? o) => JsonDocument.Parse(
        JsonSerializer.Serialize(o, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
        .RootElement;

    [Fact]
    public async Task GetMyStatus_UnknownUser_ReturnsNull()
    {
        Assert.Null(await _service.GetMyStatusAsync(999));
    }

    [Fact]
    public async Task GetList_StatusFilter_ReturnsMatchingOnly()
    {
        await SeedArtistAsync();
        var (_, _, result) = await _service.SubmitAsync(ArtistUserId, ValidRequest());
        var verificationId = ((ArtistVerification)result!).VerificationId;

        var pending = await _service.GetListAsync("PENDING");
        Assert.Single(pending);

        await _service.ReviewAsync(verificationId, AdminId, approve: true, reason: null);

        Assert.Empty(await _service.GetListAsync("PENDING"));
        Assert.Single(await _service.GetListAsync("VERIFIED"));
        Assert.Single(await _service.GetListAsync(status: null)); // tanpa filter
    }

    [Fact]
    public async Task GetList_InvalidStatusFilter_ReturnsEmpty()
    {
        await SeedArtistAsync();
        await _service.SubmitAsync(ArtistUserId, ValidRequest());

        Assert.Empty(await _service.GetListAsync("HACKED"));
    }
}
