using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Alur lamaran gig (Apply → Shortlist/Reject → Withdraw) beserta invariant
/// keamanan: artist tidak bisa mengubah lamaran milik artist lain, EO tidak bisa
/// memutuskan lamaran event milik EO lain, dan status terminal tidak bisa diubah.
/// </summary>
public class GigApplicationServiceTests : IDisposable
{
    private const int OrganizerUserId = 12;
    private const int OtherOrganizerUserId = 13;
    private const int ArtistUserId = 10;
    private const int OtherArtistUserId = 11;

    private readonly AppDbContext _context;
    private readonly GigApplicationService _service;
    private readonly Gig _gig;
    private readonly DateTime _baseDate = DateTime.UtcNow.Date.AddDays(7);

    public GigApplicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new GigApplicationService(_context, new NotificationService(_context));

        _context.Users.AddRange(
            new User { UserId = OrganizerUserId, Name = "EO", Email = "eo@test.local", PasswordHash = "x", Role = "EO" },
            new User { UserId = OtherOrganizerUserId, Name = "EO Lain", Email = "eo2@test.local", PasswordHash = "x", Role = "EO" },
            new User { UserId = ArtistUserId, Name = "Artist", Email = "artist@test.local", PasswordHash = "x", Role = "Artist" },
            new User { UserId = OtherArtistUserId, Name = "Artist Lain", Email = "artist2@test.local", PasswordHash = "x", Role = "Artist" });

        _context.Artists.AddRange(
            new Artist { ArtistId = 1, UserId = ArtistUserId, Name = "Hollow Men", Genre = "Shoegaze", City = "Jakarta" },
            new Artist { ArtistId = 2, UserId = OtherArtistUserId, Name = "Static Bloom", Genre = "Indie", City = "Bandung" });

        var venue = new Venue { Name = "Panggung", City = "Jakarta" };
        var genre = new Genre { Name = "Indie" };
        _context.Events.Add(new Event
        {
            EventId = 1,
            Name = "Festival",
            CreatedBy = OrganizerUserId,
            StartDate = _baseDate,
            EndDate = _baseDate.AddHours(6),
            Venue = venue,
            Genre = genre
        });

        _gig = new Gig
        {
            GigId = 1,
            EventId = 1,
            Name = "Opening set",
            StartAt = _baseDate.AddHours(1),
            EndAt = _baseDate.AddHours(2),
            BudgetMin = 500_000,
            BudgetMax = 1_000_000,
            RequiredArtistCount = 1,
            Status = "OPEN",
            Venue = venue,
            Genre = genre
        };
        _context.Gigs.Add(_gig);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    // ── Apply ───────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_CreatesPendingApplicationAndNotifiesOrganizer()
    {
        var (application, error, forbidden, notFound) =
            await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest { Message = "Kami siap" });

        Assert.False(forbidden);
        Assert.False(notFound);
        Assert.Null(error);
        Assert.NotNull(application);
        Assert.Equal("PENDING", application!.Status);
        Assert.Equal(1, application.ArtistId);
        Assert.Equal(1, await _context.GigApplications.CountAsync());
        Assert.True(await _context.Notifications.AnyAsync(
            n => n.UserId == OrganizerUserId && n.Type == "GIG_APPLICATION"));
    }

    [Theory]
    [InlineData("DRAFT")]
    [InlineData("CLOSED")]
    public async Task ApplyAsync_RejectsGigThatIsNotOpen(string status)
    {
        var gig = await _context.Gigs.FirstAsync(g => g.GigId == _gig.GigId);
        gig.Status = status;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (application, error, _, _) =
            await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        Assert.Null(application);
        Assert.NotNull(error);
        Assert.Empty(await _context.GigApplications.ToListAsync());
    }

    [Fact]
    public async Task ApplyAsync_RejectsPastGig()
    {
        var gig = await _context.Gigs.FirstAsync(g => g.GigId == _gig.GigId);
        gig.StartAt = DateTime.UtcNow.AddDays(-2);
        gig.EndAt = DateTime.UtcNow.AddDays(-2).AddHours(1);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (application, error, _, _) =
            await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        Assert.Null(application);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ApplyAsync_RejectsDuplicateApplication()
    {
        await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        _context.ChangeTracker.Clear();

        var (application, error, _, _) =
            await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        Assert.Null(application);
        Assert.NotNull(error);
        Assert.Equal(1, await _context.GigApplications.CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_ReopensWithdrawnApplication()
    {
        var (first, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        var applicationId = first!.GigApplicationId;

        var (withdrawn, _, _, _) = await _service.WithdrawAsync(applicationId, ArtistUserId);
        Assert.True(withdrawn);
        _context.ChangeTracker.Clear();

        var (reapplied, error, _, _) =
            await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest { Message = "Masih berminat" });

        Assert.Null(error);
        Assert.NotNull(reapplied);
        Assert.Equal("PENDING", reapplied!.Status);
        // Tetap satu baris lamaran — unique (GigId, ArtistId) tidak dilanggar.
        Assert.Equal(1, await _context.GigApplications.CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_ForbidsUserWithoutArtistProfile()
    {
        var (application, error, forbidden, _) =
            await _service.ApplyAsync(_gig.GigId, OrganizerUserId, new ApplyToGigRequest());

        Assert.Null(application);
        Assert.True(forbidden);
        Assert.NotNull(error);
    }

    // ── Withdraw ────────────────────────────────────────

    [Fact]
    public async Task WithdrawAsync_ForbidsAnotherArtist()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        var (success, error, forbidden, _) =
            await _service.WithdrawAsync(application!.GigApplicationId, OtherArtistUserId);

        Assert.False(success);
        Assert.True(forbidden);
        Assert.NotNull(error);
        Assert.Equal("PENDING", (await _context.GigApplications.FirstAsync()).Status);
    }

    [Fact]
    public async Task WithdrawAsync_RejectsApplicationThatAlreadyDecided()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        await _service.DecideAsync(application!.GigApplicationId, OrganizerUserId, "EO", shortlist: false);
        _context.ChangeTracker.Clear();

        var (success, error, _, _) =
            await _service.WithdrawAsync(application.GigApplicationId, ArtistUserId);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Equal("REJECTED", (await _context.GigApplications.FirstAsync()).Status);
    }

    // ── EO decisions ────────────────────────────────────

    [Fact]
    public async Task DecideAsync_Shortlist_MarksApplicationAndNotifiesArtist()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        var (decided, error, forbidden, _) =
            await _service.DecideAsync(application!.GigApplicationId, OrganizerUserId, "EO", shortlist: true);

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.Equal("SHORTLISTED", decided!.Status);
        Assert.True(await _context.Notifications.AnyAsync(
            n => n.UserId == ArtistUserId && n.Type == "GIG_APPLICATION_UPDATE"));
    }

    [Fact]
    public async Task DecideAsync_Reject_AllowedFromShortlisted()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        await _service.DecideAsync(application!.GigApplicationId, OrganizerUserId, "EO", shortlist: true);
        _context.ChangeTracker.Clear();

        var (decided, error, _, _) =
            await _service.DecideAsync(application.GigApplicationId, OrganizerUserId, "EO", shortlist: false);

        Assert.Null(error);
        Assert.Equal("REJECTED", decided!.Status);
    }

    [Fact]
    public async Task DecideAsync_CannotShortlistAfterDecision()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        await _service.DecideAsync(application!.GigApplicationId, OrganizerUserId, "EO", shortlist: false);
        _context.ChangeTracker.Clear();

        var (decided, error, _, _) =
            await _service.DecideAsync(application.GigApplicationId, OrganizerUserId, "EO", shortlist: true);

        Assert.Null(decided);
        Assert.NotNull(error);
        Assert.Equal("REJECTED", (await _context.GigApplications.FirstAsync()).Status);
    }

    [Fact]
    public async Task DecideAsync_ForbidsAnotherOrganizer()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        var (decided, error, forbidden, _) =
            await _service.DecideAsync(application!.GigApplicationId, OtherOrganizerUserId, "EO", shortlist: true);

        Assert.Null(decided);
        Assert.True(forbidden);
        Assert.NotNull(error);
        Assert.Equal("PENDING", (await _context.GigApplications.FirstAsync()).Status);
    }

    [Fact]
    public async Task DecideAsync_AdminCanDecideAnyGig()
    {
        var (application, _, _, _) = await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());

        var (decided, error, forbidden, _) =
            await _service.DecideAsync(application!.GigApplicationId, OtherOrganizerUserId, "Admin", shortlist: true);

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.Equal("SHORTLISTED", decided!.Status);
    }

    [Fact]
    public async Task GetForGigAsync_IsScopedToEventOwner()
    {
        await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        _context.ChangeTracker.Clear();

        var (forOther, _, forbidden, _) =
            await _service.GetForGigAsync(_gig.GigId, OtherOrganizerUserId, "EO");
        Assert.Null(forOther);
        Assert.True(forbidden);

        var (owned, error, _, _) =
            await _service.GetForGigAsync(_gig.GigId, OrganizerUserId, "EO");
        Assert.Null(error);
        Assert.Single(owned!);
    }

    [Fact]
    public async Task GetMyAsync_ReturnsOnlyOwnApplications()
    {
        await _service.ApplyAsync(_gig.GigId, ArtistUserId, new ApplyToGigRequest());
        _context.ChangeTracker.Clear();

        var mine = await _service.GetMyAsync(ArtistUserId);
        var others = await _service.GetMyAsync(OtherArtistUserId);

        Assert.Single(mine);
        Assert.Empty(others);
    }
}
