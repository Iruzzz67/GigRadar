using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

public class GigServiceTests : IDisposable
{
    private const int OwnerId = 12;
    private const int OtherOwnerId = 13;
    private const int ArtistUserId = 20;
    private readonly AppDbContext _context;
    private readonly GigService _service;
    private readonly GigApplicationService _applications;
    private readonly Event _event;

    public GigServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new GigService(_context);
        _applications = new GigApplicationService(_context, new NotificationService(_context));

        var venue = new Venue { Name = "Panggung", City = "Jakarta" };
        var genre = new Genre { Name = "Indie" };
        _event = new Event
        {
            Name = "Festival",
            CreatedBy = OwnerId,
            StartDate = new DateTime(2026, 10, 1, 18, 0, 0),
            EndDate = new DateTime(2026, 10, 1, 23, 0, 0),
            Venue = venue,
            Genre = genre
        };
        _context.Events.Add(_event);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    [Fact]
    public async Task CreateAsync_PersistsGigUsingEventVenueAndGenre()
    {
        var request = MakeRequest();

        var (gig, error, forbidden, notFound) =
            await _service.CreateAsync(_event.EventId, OwnerId, "EO", request);

        Assert.False(forbidden);
        Assert.False(notFound);
        Assert.Null(error);
        Assert.NotNull(gig);
        Assert.Equal(_event.VenueId, gig!.VenueId);
        Assert.Equal(_event.GenreId, gig.GenreId);
        Assert.Equal("OPEN", gig.Status);
        Assert.Equal(1, await _context.Gigs.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsAnotherOrganizersEvent()
    {
        var (gig, error, forbidden, _) =
            await _service.CreateAsync(_event.EventId, OtherOwnerId, "EO", MakeRequest());

        Assert.True(forbidden);
        Assert.Null(gig);
        Assert.NotNull(error);
        Assert.Empty(await _context.Gigs.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidScheduleAndBudget()
    {
        var request = MakeRequest();
        request.StartAt = _event.EndDate.AddMinutes(-10);
        request.EndAt = _event.EndDate.AddMinutes(10);
        request.BudgetMin = 500;
        request.BudgetMax = 100;

        var (gig, error, _, _) =
            await _service.CreateAsync(_event.EventId, OwnerId, "EO", request);

        Assert.Null(gig);
        Assert.NotNull(error);
        Assert.Empty(await _context.Gigs.ToListAsync());
    }

    [Fact]
    public async Task GetForEventAsync_IsScopedToEventOwner()
    {
        var (items, error, forbidden) =
            await _service.GetForEventAsync(_event.EventId, OtherOwnerId, "EO");

        Assert.Null(items);
        Assert.NotNull(error);
        Assert.True(forbidden);
    }

    [Fact]
    public async Task GetForEventAsync_AdminCanManageAnyEvent()
    {
        var (items, error, forbidden) =
            await _service.GetForEventAsync(_event.EventId, OtherOwnerId, "Admin");

        Assert.NotNull(items);
        Assert.Null(error);
        Assert.False(forbidden);
    }

    [Fact]
    public async Task ChangeStatusAsync_FollowsLifecycle()
    {
        var (gig, _, _, _) = await _service.CreateAsync(
            _event.EventId, OwnerId, "EO", MakeRequest(status: "DRAFT"));
        var gigId = gig!.GigId;

        // DRAFT tidak boleh langsung CLOSED.
        var (closed, closedError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "CLOSED");
        Assert.Null(closed);
        Assert.NotNull(closedError);

        // DRAFT → OPEN diizinkan.
        var (opened, openError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "OPEN");
        Assert.Null(openError);
        Assert.Equal("OPEN", opened!.Status);

        // Status yang sama ditolak (bukan transisi).
        var (again, againError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "OPEN");
        Assert.Null(again);
        Assert.NotNull(againError);

        // OPEN → CLOSED dan CLOSED → OPEN diizinkan.
        var (closedOk, closedOkError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "CLOSED");
        Assert.Null(closedOkError);
        Assert.Equal("CLOSED", closedOk!.Status);

        var (reopened, reopenedError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "OPEN");
        Assert.Null(reopenedError);
        Assert.Equal("OPEN", reopened!.Status);

        // Status tak dikenal ditolak.
        var (unknown, unknownError, _, _) =
            await _service.ChangeStatusAsync(_event.EventId, gigId, OwnerId, "EO", "PUBLISHED");
        Assert.Null(unknown);
        Assert.NotNull(unknownError);
    }

    [Fact]
    public async Task ChangeStatusAsync_ForbidsAnotherOrganizer()
    {
        var (gig, _, _, _) = await _service.CreateAsync(_event.EventId, OwnerId, "EO", MakeRequest());

        var (_, error, forbidden, _) =
            await _service.ChangeStatusAsync(_event.EventId, gig!.GigId, OtherOwnerId, "EO", "CLOSED");

        Assert.True(forbidden);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task UpdateAsync_AppliesChangesAndValidatesAgainstEventWindow()
    {
        var (gig, _, _, _) = await _service.CreateAsync(_event.EventId, OwnerId, "EO", MakeRequest());
        var request = MakeRequest();
        request.Name = "Slot utama";
        request.StartAt = _event.StartDate.AddHours(2);
        request.EndAt = _event.StartDate.AddHours(3);
        request.BudgetMin = 750_000;
        request.BudgetMax = 1_500_000;

        var (updated, error, _, _) = await _service.UpdateAsync(
            _event.EventId, gig!.GigId, OwnerId, "EO", request);
        Assert.Null(error);
        Assert.Equal("Slot utama", updated!.Name);
        Assert.Equal(1_500_000, updated.BudgetMax);

        // Jadwal di luar jendela event ditolak, dan data lama tidak berubah.
        request.StartAt = _event.StartDate.AddDays(-1);
        request.EndAt = _event.StartDate.AddMinutes(30);
        var (rejected, rejectedError, _, _) = await _service.UpdateAsync(
            _event.EventId, gig.GigId, OwnerId, "EO", request);
        Assert.Null(rejected);
        Assert.NotNull(rejectedError);
        Assert.Equal("Slot utama", (await _context.Gigs.FirstAsync(g => g.GigId == gig.GigId)).Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesGigAndItsApplications()
    {
        SeedArtistUser();
        var (gig, _, _, _) = await _service.CreateAsync(_event.EventId, OwnerId, "EO", MakeRequest());
        await _applications.ApplyAsync(gig!.GigId, ArtistUserId, new ApplyToGigRequest());
        _context.ChangeTracker.Clear();

        var (success, message, forbidden, notFound) =
            await _service.DeleteAsync(_event.EventId, gig.GigId, OwnerId, "EO");

        Assert.True(success);
        Assert.False(forbidden);
        Assert.False(notFound);
        Assert.NotNull(message);
        Assert.Empty(await _context.Gigs.ToListAsync());
        Assert.Empty(await _context.GigApplications.ToListAsync());
    }

    [Fact]
    public async Task GetOpenGigsAsync_ExcludesDraftClosedAndPastGigs()
    {
        var now = DateTime.UtcNow;
        _context.Gigs.AddRange(
            new Gig { EventId = _event.EventId, Name = "Open future", StartAt = now.AddDays(2), EndAt = now.AddDays(2).AddHours(1), Status = "OPEN" },
            new Gig { EventId = _event.EventId, Name = "Draft", StartAt = now.AddDays(2), EndAt = now.AddDays(2).AddHours(1), Status = "DRAFT" },
            new Gig { EventId = _event.EventId, Name = "Closed", StartAt = now.AddDays(2), EndAt = now.AddDays(2).AddHours(1), Status = "CLOSED" },
            new Gig { EventId = _event.EventId, Name = "Past", StartAt = now.AddDays(-3), EndAt = now.AddDays(-3).AddHours(1), Status = "OPEN" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _service.GetOpenGigsAsync(OwnerId);

        Assert.Single(result);
        Assert.Equal("Open future", result[0].Name);
    }

    [Fact]
    public async Task GetOpenGigsAsync_FiltersAndMarksAppliedForArtist()
    {
        SeedArtistUser();
        var now = DateTime.UtcNow;
        _context.Gigs.Add(new Gig
        {
            EventId = _event.EventId,
            Name = "Opening set",
            StartAt = now.AddDays(2),
            EndAt = now.AddDays(2).AddHours(1),
            Status = "OPEN",
            VenueId = _event.VenueId,
            GenreId = _event.GenreId,
            BudgetMin = 2_000_000,
            BudgetMax = 3_000_000
        });
        await _context.SaveChangesAsync();
        var gigId = (await _context.Gigs.FirstAsync(g => g.Name == "Opening set")).GigId;
        _context.ChangeTracker.Clear();

        var (application, error, _, _) =
            await _applications.ApplyAsync(gigId, ArtistUserId, new ApplyToGigRequest());
        Assert.Null(error);
        Assert.NotNull(application);
        _context.ChangeTracker.Clear();

        var all = await _service.GetOpenGigsAsync(ArtistUserId);
        Assert.Single(all);
        Assert.True(all[0].HasApplied);
        Assert.Equal(1, all[0].ApplicantCount);
        Assert.Equal("Jakarta", all[0].City);
        Assert.Equal("Indie", all[0].GenreName);
        Assert.Equal("Festival", all[0].EventName);

        Assert.Empty(await _service.GetOpenGigsAsync(ArtistUserId, q: "tidak-ada"));
        Assert.Single(await _service.GetOpenGigsAsync(ArtistUserId, q: "Opening"));
        Assert.Single(await _service.GetOpenGigsAsync(ArtistUserId, city: "Jakarta"));
        Assert.Empty(await _service.GetOpenGigsAsync(ArtistUserId, city: "Surabaya"));
        Assert.Single(await _service.GetOpenGigsAsync(ArtistUserId, genreId: _event.GenreId));
        Assert.Empty(await _service.GetOpenGigsAsync(ArtistUserId, genreId: 999));
        Assert.Single(await _service.GetOpenGigsAsync(ArtistUserId, maxBudget: 2_500_000));
        Assert.Empty(await _service.GetOpenGigsAsync(ArtistUserId, maxBudget: 1_000_000));
    }

    /// <summary>Profil artist untuk user uji (apply butuh akun artist yang sudah punya profil).</summary>
    private void SeedArtistUser()
    {
        _context.Users.Add(new User
        {
            UserId = ArtistUserId,
            Name = "Artist Uji",
            Email = "artist-uji@test.local",
            PasswordHash = "x",
            Role = "Artist"
        });
        _context.Artists.Add(new Artist
        {
            ArtistId = 100,
            UserId = ArtistUserId,
            Name = "Hollow Men",
            Genre = "Shoegaze",
            City = "Jakarta"
        });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private CreateGigRequest MakeRequest(string status = "OPEN") => new()
    {
        Name = "Opening set",
        Requirements = "Bring own instruments",
        StartAt = new DateTime(2026, 10, 1, 19, 0, 0),
        EndAt = new DateTime(2026, 10, 1, 20, 0, 0),
        BudgetMin = 500000,
        BudgetMax = 1000000,
        RequiredArtistCount = 1,
        Status = status
    };
}
