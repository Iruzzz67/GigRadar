using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test EventService memakai EF InMemory — fokus pada aturan bisnis:
/// transisi status, ownership Admin/EO, nearby, dan ringkasan EO.
/// </summary>
public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EventService _service;

    // Id user seed: 1 = Admin, 2 = EO.
    private const int AdminId = 1;
    private const int EoId = 2;
    private const int OtherEoId = 20;

    public EventServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new EventService(_context);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private async Task<Event> SeedEventAsync(Event? evt = null)
    {
        var entity = evt ?? TestData.MakeEvent();
        _context.Events.Add(entity);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return entity;
    }

    // ── Create ───────────────────────────────────────────

    [Fact]
    public async Task CreateEvent_SetsDefaultsAndPersists()
    {
        var created = await _service.CreateEventAsync(TestData.MakeEvent(name: "Skena Night"));

        Assert.NotNull(created);
        Assert.True(created!.EventId > 0);
        Assert.Equal(0, created.ViewsCount);       // tidak bisa diatur dari body
        Assert.Equal(0, created.SavesCount);
        Assert.Equal("Skena Night", created.Name);
    }

    [Fact]
    public async Task CreateEvent_EmptyStatus_DefaultsToPublished()
    {
        var evt = TestData.MakeEvent(status: "");
        var created = await _service.CreateEventAsync(evt);

        Assert.NotNull(created);
        Assert.Equal("Published", created!.Status);
    }

    // ── Read: nearby / tonight / weekend ────────────────

    [Fact]
    public async Task GetNearbyEvents_ExcludesDraftAndOutOfRange()
    {
        await SeedEventAsync(TestData.MakeEvent(name: "Dekat", lat: -6.21, lng: 106.81));                       // ±2 km
        await SeedEventAsync(TestData.MakeEvent(name: "Jauh", lat: -6.9, lng: 107.6));                          // Bandung
        await SeedEventAsync(TestData.MakeEvent(name: "Draft", lat: -6.21, lng: 106.81, status: "Draft"));

        var result = await _service.GetNearbyEventsAsync(-6.2, 106.8, radiusKm: 25);

        var ev = Assert.Single(result);
        Assert.Equal("Dekat", ev.Name);
    }

    [Fact]
    public async Task GetTonightEvents_OnlyPublishedToday()
    {
        await SeedEventAsync(TestData.MakeEvent(name: "Malam Ini", start: DateTime.Today.AddHours(20)));
        await SeedEventAsync(TestData.MakeEvent(name: "Besok", start: DateTime.Today.AddDays(1).AddHours(20)));
        await SeedEventAsync(TestData.MakeEvent(name: "Draft Malam Ini", status: "Draft",
            start: DateTime.Today.AddHours(21)));

        var result = await _service.GetTonightEventsAsync();

        var ev = Assert.Single(result);
        Assert.Equal("Malam Ini", ev.Name);
    }

    [Fact]
    public async Task GetWeekendEvents_WithinSevenDaysOnly()
    {
        await SeedEventAsync(TestData.MakeEvent(name: "Dalam Seminggu", start: DateTime.Today.AddDays(3)));
        await SeedEventAsync(TestData.MakeEvent(name: "Luar Seminggu", start: DateTime.Today.AddDays(10)));

        var result = await _service.GetWeekendEventsAsync();

        var ev = Assert.Single(result);
        Assert.Equal("Dalam Seminggu", ev.Name);
    }

    // ── Update: ownership ────────────────────────────────

    [Fact]
    public async Task UpdateEvent_ByOwner_Succeeds()
    {
        var seeded = await SeedEventAsync();

        var (success, error, forbidden) = await _service.UpdateEventAsync(
            seeded.EventId, TestData.MakeEvent(name: "Nama Baru"), EoId, "EO");

        Assert.True(success);
        Assert.Null(error);
        Assert.False(forbidden);

        var updated = await _service.GetEventByIdAsync(seeded.EventId);
        Assert.Equal("Nama Baru", updated!.Name);
    }

    [Fact]
    public async Task UpdateEvent_ByOtherEo_Forbidden()
    {
        var seeded = await SeedEventAsync();

        var (success, error, forbidden) = await _service.UpdateEventAsync(
            seeded.EventId, TestData.MakeEvent(name: "Direbut"), OtherEoId, "EO");

        Assert.False(success);
        Assert.True(forbidden);
        Assert.Contains("akses", error);

        // Data tidak berubah.
        var unchanged = await _service.GetEventByIdAsync(seeded.EventId);
        Assert.Equal("Gig Test", unchanged!.Name);
    }

    [Fact]
    public async Task UpdateEvent_ByAdmin_AlwaysAllowed()
    {
        var seeded = await SeedEventAsync(TestData.MakeEvent(createdBy: OtherEoId));

        var (success, error, forbidden) = await _service.UpdateEventAsync(
            seeded.EventId, TestData.MakeEvent(name: "Admin Edit"), AdminId, "Admin");

        Assert.True(success);
        Assert.Null(error);
        Assert.False(forbidden);
    }

    [Fact]
    public async Task UpdateEvent_MissingId_ReturnsNotFound()
    {
        var (success, error, forbidden) = await _service.UpdateEventAsync(
            9999, TestData.MakeEvent(), EoId, "EO");

        Assert.False(success);
        Assert.False(forbidden);
        Assert.Equal("Event tidak ditemukan", error);
    }

    // ── Status transition ────────────────────────────────

    [Fact]
    public async Task ChangeStatus_AllTransitions_Accepted()
    {
        // Rantai transisi yang dipakai UI EO: Published → SoldOut → Published → Completed → Draft.
        var seeded = await SeedEventAsync();

        foreach (var status in new[] { "SoldOut", "Published", "Completed", "Draft" })
        {
            var (updated, error, forbidden) = await _service.ChangeEventStatusAsync(
                seeded.EventId, EoId, "EO", status);

            Assert.Null(error);
            Assert.False(forbidden);
            Assert.Equal(status, updated!.Status);
        }
    }

    [Theory]
    [InlineData("Cancelled", "Status tidak valid")]
    [InlineData("Publish", "Status tidak valid")]
    [InlineData("published extra", "Status tidak valid")]
    [InlineData("", "Status wajib diisi")]
    public async Task ChangeStatus_InvalidStatus_Rejected(string status, string expectedError)
    {
        var seeded = await SeedEventAsync();

        var (updated, error, forbidden) = await _service.ChangeEventStatusAsync(
            seeded.EventId, EoId, "EO", status);

        Assert.Null(updated);
        Assert.False(forbidden);
        Assert.Contains(expectedError, error);

        var unchanged = await _service.GetEventByIdAsync(seeded.EventId);
        Assert.Equal("Published", unchanged!.Status);
    }

    [Fact]
    public async Task ChangeStatus_ByOtherEo_Forbidden()
    {
        var seeded = await SeedEventAsync();

        var (_, error, forbidden) = await _service.ChangeEventStatusAsync(
            seeded.EventId, OtherEoId, "EO", "SoldOut");

        Assert.True(forbidden);
        Assert.Contains("akses", error);
    }

    [Fact]
    public async Task ChangeStatus_StatusTrimmed()
    {
        var seeded = await SeedEventAsync();

        var (updated, error, _) = await _service.ChangeEventStatusAsync(
            seeded.EventId, EoId, "EO", "  SoldOut  ");

        Assert.Null(error);
        Assert.Equal("SoldOut", updated!.Status);
    }

    // ── Delete ───────────────────────────────────────────

    [Fact]
    public async Task DeleteEvent_ByOwner_RemovesEventAndRelations()
    {
        var seeded = await SeedEventAsync();
        _context.Tickets.Add(TestData.MakeTicket(seeded.EventId));
        _context.Favorites.Add(new Favorite { UserId = 10, EventId = seeded.EventId });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (deleted, error, forbidden) = await _service.DeleteEventAsync(seeded.EventId, EoId, "EO");

        Assert.True(deleted);
        Assert.Null(error);
        Assert.False(forbidden);
        Assert.Null(await _service.GetEventByIdAsync(seeded.EventId));
        Assert.Empty(await _context.Tickets.Where(t => t.EventId == seeded.EventId).ToListAsync());
        Assert.Empty(await _context.Favorites.Where(f => f.EventId == seeded.EventId).ToListAsync());
    }

    [Fact]
    public async Task DeleteEvent_ByOtherEo_Forbidden()
    {
        var seeded = await SeedEventAsync();

        var (deleted, error, forbidden) = await _service.DeleteEventAsync(seeded.EventId, OtherEoId, "EO");

        Assert.False(deleted);
        Assert.True(forbidden);
        Assert.NotNull(await _service.GetEventByIdAsync(seeded.EventId));
    }

    // ── Summary (EO) ─────────────────────────────────────

    [Fact]
    public async Task GetManagedSummary_AggregatesTicketsExcludingCancelled()
    {
        var seeded = await SeedEventAsync();
        _context.Tickets.Add(TestData.MakeTicket(seeded.EventId, userId: 10, price: 100_000));
        _context.Tickets.Add(TestData.MakeTicket(seeded.EventId, userId: 11, price: 100_000));
        _context.Tickets.Add(TestData.MakeTicket(seeded.EventId, userId: 12, price: 100_000, status: "Cancelled"));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var summary = await _service.GetManagedSummaryAsync(EoId, "EO");
        var json = JsonSerializer.Serialize(summary);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(1, root.GetProperty("totalEvents").GetInt32());
        Assert.Equal(2, root.GetProperty("totalTicketsSold").GetInt32());   // cancelled tidak dihitung
        Assert.Equal(200_000m, root.GetProperty("totalRevenue").GetDecimal());
    }

    [Fact]
    public async Task GetManagedEvents_EoSeesOnlyOwn_AdminSeesAll()
    {
        await SeedEventAsync(TestData.MakeEvent(name: "Milik EO", createdBy: EoId));
        await SeedEventAsync(TestData.MakeEvent(name: "Milik EO Lain", createdBy: OtherEoId));

        var eoView = await _service.GetManagedEventsAsync(EoId, "EO");
        var adminView = await _service.GetManagedEventsAsync(AdminId, "Admin");

        var eoEv = Assert.Single(eoView);
        Assert.Equal("Milik EO", eoEv.Name);
        Assert.Equal(2, adminView.Count);
    }
}
