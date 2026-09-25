using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test TicketService — fokus pada aturan bisnis yang paling sensitif:
/// check-in QR (Bab 14 + kepemilikan event §24), pembelian tiket, dan stok.
///
/// Memakai SQLite in-memory (bukan provider InMemory) karena pengurangan stok
/// memakai ExecuteUpdate — operasi relational yang tidak didukung InMemory.
/// </summary>
public class TicketServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TicketService _service;

    // Id user seed: 1 = Admin, 2 = EO (pemilik event demo 1), 20 = EO lain.
    private const int AdminId = 1;
    private const int EoId = 2;
    private const int OtherEoId = 20;

    // SQLite menegakkan foreign key, jadi pembeli harus benar-benar ada di tabel Users
    // (provider InMemory tidak peduli — karena itu test ini pakai id hasil insert).
    private readonly int _buyer1;
    private readonly int _buyer2;

    public TicketServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _service = new TicketService(_context);

        _buyer1 = AddUser("Pembeli Satu", "pembeli1@uji.test");
        _buyer2 = AddUser("Pembeli Dua", "pembeli2@uji.test");
    }

    private int AddUser(string name, string email)
    {
        var user = new User { Name = name, Email = email, PasswordHash = "x", Role = "User" };
        _context.Users.Add(user);
        _context.SaveChanges();
        return user.UserId;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // ── Helper ───────────────────────────────────────────

    /// <summary>Event seed milik EO demo (EventId 1, CreatedBy = 2).</summary>
    private async Task<Event> SeedOwnedEventAsync()
    {
        var evt = _context.Events.First(e => e.EventId == 1);
        Assert.Equal(EoId, evt.CreatedBy);
        await Task.CompletedTask;
        return evt;
    }

    private async Task<Ticket> SeedTicketAsync(int eventId, string qr, string status = "Active")
    {
        var ticket = TestData.MakeTicket(eventId, userId: _buyer1);
        ticket.QRCode = qr;
        ticket.Status = status;
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return ticket;
    }

    private async Task<EventTicketType> SeedTicketTypeAsync(int eventId, int stock, decimal price = 75_000)
    {
        var type = new EventTicketType
        {
            EventId = eventId,
            Name = "Festival",
            Description = "",
            Price = price,
            Stock = stock,
            SortOrder = 1
        };
        _context.EventTicketTypes.Add(type);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return type;
    }

    private Task<int> StockOfAsync(int typeId) =>
        _context.EventTicketTypes.Where(t => t.EventTicketTypeId == typeId)
            .Select(t => t.Stock).FirstAsync();

    // ── Check-in: kepemilikan event (§24) ────────────────

    [Fact]
    public async Task CheckIn_OwnEvent_MarksTicketUsed()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-OWN-1");

        var (ticket, error, forbidden) = await _service.CheckInTicketAsync("QR-OWN-1", evt.EventId, EoId, "EO");

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.NotNull(ticket);
        Assert.Equal("Used", ticket!.Status);

        var (checkedIn, total) = await _service.GetCheckInCountsAsync(evt.EventId);
        Assert.Equal(1, checkedIn);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task CheckIn_ByOtherEo_ForbiddenAndTicketUntouched()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-OTHER-1");

        var (ticket, error, forbidden) = await _service.CheckInTicketAsync("QR-OTHER-1", evt.EventId, OtherEoId, "EO");

        Assert.Null(ticket);
        Assert.True(forbidden);
        Assert.Contains("akses", error);

        var stored = await _context.Tickets.FirstAsync(t => t.QRCode == "QR-OTHER-1");
        Assert.Equal("Active", stored.Status);   // EO lain tidak bisa membakar tiket event orang
    }

    [Fact]
    public async Task CheckIn_TicketFromDifferentEvent_Rejected()
    {
        // EO yang sama memiliki event lain; QR milik event 1 tidak boleh lolos di event 2.
        var ownEvent = await SeedOwnedEventAsync();
        var otherEvent = TestData.MakeEvent(name: "Event Lain", createdBy: EoId);
        _context.Events.Add(otherEvent);
        await _context.SaveChangesAsync();

        await SeedTicketAsync(ownEvent.EventId, "QR-SCOPE-1");

        var (ticket, error, forbidden) = await _service.CheckInTicketAsync(
            "QR-SCOPE-1", otherEvent.EventId, EoId, "EO");

        Assert.Null(ticket);
        Assert.False(forbidden);
        Assert.Contains("tidak dikenali", error);

        var stored = await _context.Tickets.FirstAsync(t => t.QRCode == "QR-SCOPE-1");
        Assert.Equal("Active", stored.Status);
    }

    [Fact]
    public async Task CheckIn_AlreadyUsed_Rejected()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-USED-1", status: "Used");

        var (ticket, error, _) = await _service.CheckInTicketAsync("QR-USED-1", evt.EventId, EoId, "EO");

        Assert.Null(ticket);
        Assert.Contains("sudah pernah dipakai", error);
    }

    [Fact]
    public async Task CheckIn_CancelledTicket_Rejected()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-CANCEL-1", status: "Cancelled");

        var (ticket, error, _) = await _service.CheckInTicketAsync("QR-CANCEL-1", evt.EventId, EoId, "EO");

        Assert.Null(ticket);
        Assert.Contains("tidak berstatus aktif", error);
    }

    [Fact]
    public async Task CheckIn_Admin_AllowedOnAnyEvent()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-ADMIN-1");

        var (ticket, error, forbidden) = await _service.CheckInTicketAsync("QR-ADMIN-1", evt.EventId, AdminId, "Admin");

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.Equal("Used", ticket!.Status);
    }

    [Fact]
    public async Task CheckIn_EmptyQrOrEventId_RejectedWithoutForbidden()
    {
        var evt = await SeedOwnedEventAsync();

        var (ticketA, errorA, forbiddenA) = await _service.CheckInTicketAsync("   ", evt.EventId, EoId, "EO");
        var (ticketB, errorB, forbiddenB) = await _service.CheckInTicketAsync("QR-ANY", 0, EoId, "EO");

        Assert.Null(ticketA);
        Assert.False(forbiddenA);
        Assert.Contains("QR", errorA);

        Assert.Null(ticketB);
        Assert.False(forbiddenB);
        Assert.Contains("Event", errorB);
    }

    // ── Daftar peserta (§14/§30) ─────────────────────────

    [Fact]
    public async Task GetAttendees_ByOwner_ReturnsOnlyThatEventTickets()
    {
        var ownEvent = await SeedOwnedEventAsync();
        var otherEvent = TestData.MakeEvent(name: "Event Lain", createdBy: EoId);
        _context.Events.Add(otherEvent);
        await _context.SaveChangesAsync();

        await SeedTicketAsync(ownEvent.EventId, "QR-A-1");
        await SeedTicketAsync(ownEvent.EventId, "QR-A-2", status: "Used");
        await SeedTicketAsync(otherEvent.EventId, "QR-B-1");

        var (attendees, error, forbidden) = await _service.GetEventAttendeesAsync(ownEvent.EventId, EoId, "EO");

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.NotNull(attendees);
        Assert.Equal(2, attendees!.Count);
        Assert.All(attendees, t => Assert.Equal(ownEvent.EventId, t.EventId));
    }

    [Fact]
    public async Task GetAttendees_ByOtherEo_Forbidden()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-FORBID-1");

        var (attendees, error, forbidden) = await _service.GetEventAttendeesAsync(evt.EventId, OtherEoId, "EO");

        Assert.Null(attendees);
        Assert.True(forbidden);
        Assert.Contains("akses", error);
    }

    [Fact]
    public async Task GetAttendees_AdminAllowed_AndEmptyEventReturnsEmptyList()
    {
        var evt = await SeedOwnedEventAsync();

        var (empty, error, forbidden) = await _service.GetEventAttendeesAsync(evt.EventId, AdminId, "Admin");

        Assert.Null(error);
        Assert.False(forbidden);
        Assert.Empty(empty!);

        var (missing, _, missingForbidden) = await _service.GetEventAttendeesAsync(0, AdminId, "Admin");
        Assert.Null(missing);
        Assert.False(missingForbidden);
    }

    [Fact]
    public async Task GetAttendees_AfterCheckIn_SummaryCountsMatch()
    {
        var evt = await SeedOwnedEventAsync();
        await SeedTicketAsync(evt.EventId, "QR-SUM-1");
        await SeedTicketAsync(evt.EventId, "QR-SUM-2");

        await _service.CheckInTicketAsync("QR-SUM-1", evt.EventId, EoId, "EO");

        var (attendees, _, _) = await _service.GetEventAttendeesAsync(evt.EventId, EoId, "EO");
        var (checkedIn, total) = await _service.GetCheckInCountsAsync(evt.EventId);

        Assert.Equal(2, attendees!.Count);
        Assert.Equal(2, total);
        Assert.Equal(1, checkedIn);
        Assert.Single(attendees, t => t.Status == "Used");
    }

    // ── Pembelian & stok ─────────────────────────────────

    [Fact]
    public async Task Purchase_DecrementsStockAndUsesServerPrice()
    {
        var evt = await SeedOwnedEventAsync();
        var type = await SeedTicketTypeAsync(evt.EventId, stock: 5, price: 42_000);

        var (ticket, error) = await _service.PurchaseTicketAsync(
            userId: _buyer1, eventId: evt.EventId, ticketTypeId: type.EventTicketTypeId,
            buyerName: "Pembeli Uji", buyerPhone: "08120000000",
            buyerEmail: "pembeli@uji.test", buyerDateOfBirth: new DateTime(2000, 1, 1));

        Assert.Null(error);
        Assert.NotNull(ticket);
        Assert.Equal(42_000m, ticket!.Price);          // harga dari server, bukan dari client
        Assert.Equal("Festival", ticket.TicketType);
        Assert.Equal("Active", ticket.Status);
        Assert.False(string.IsNullOrWhiteSpace(ticket.QRCode));
        Assert.Equal(4, await StockOfAsync(type.EventTicketTypeId));
    }

    [Fact]
    public async Task Purchase_OnLastStock_SecondBuyerRejectedAndStockNotNegative()
    {
        var evt = await SeedOwnedEventAsync();
        var type = await SeedTicketTypeAsync(evt.EventId, stock: 1);

        var (first, firstError) = await _service.PurchaseTicketAsync(
            _buyer1, evt.EventId, type.EventTicketTypeId,
            "Pembeli Satu", "08120000001", "satu@uji.test", new DateTime(2000, 1, 1));

        var (second, secondError) = await _service.PurchaseTicketAsync(
            _buyer2, evt.EventId, type.EventTicketTypeId,
            "Pembeli Dua", "08120000002", "dua@uji.test", new DateTime(2000, 1, 1));

        Assert.Null(firstError);
        Assert.NotNull(first);
        Assert.Null(second);
        Assert.Contains("habis", secondError);
        Assert.Equal(0, await StockOfAsync(type.EventTicketTypeId));
    }

    [Fact]
    public async Task Purchase_UnderageBuyer_Rejected()
    {
        var evt = await SeedOwnedEventAsync();
        var type = await SeedTicketTypeAsync(evt.EventId, stock: 3);

        var (ticket, error) = await _service.PurchaseTicketAsync(
            _buyer1, evt.EventId, type.EventTicketTypeId,
            "Anak Baru", "08120000003", "anak@uji.test",
            buyerDateOfBirth: DateTime.Today.AddYears(-15));

        Assert.Null(ticket);
        Assert.Contains("umur", error);
        Assert.Equal(3, await StockOfAsync(type.EventTicketTypeId));   // stok tidak berkurang saat validasi gagal
    }

    [Fact]
    public async Task Purchase_UnpublishedEvent_Rejected()
    {
        var draft = TestData.MakeEvent(name: "Draft Show", status: "Draft", createdBy: EoId);
        _context.Events.Add(draft);
        await _context.SaveChangesAsync();
        var type = await SeedTicketTypeAsync(draft.EventId, stock: 3);

        var (ticket, error) = await _service.PurchaseTicketAsync(
            _buyer1, draft.EventId, type.EventTicketTypeId,
            "Pembeli Uji", "08120000000", "pembeli@uji.test", new DateTime(2000, 1, 1));

        Assert.Null(ticket);
        Assert.Contains("tidak menerima pembelian", error);
        Assert.Equal(3, await StockOfAsync(type.EventTicketTypeId));
    }

    [Fact]
    public async Task Purchase_TicketTypeFromAnotherEvent_Rejected()
    {
        var evt = await SeedOwnedEventAsync();
        var other = TestData.MakeEvent(name: "Event Lain", createdBy: EoId);
        _context.Events.Add(other);
        await _context.SaveChangesAsync();

        var foreignType = await SeedTicketTypeAsync(other.EventId, stock: 3);

        var (ticket, error) = await _service.PurchaseTicketAsync(
            _buyer1, evt.EventId, foreignType.EventTicketTypeId,
            "Pembeli Uji", "08120000000", "pembeli@uji.test", new DateTime(2000, 1, 1));

        Assert.Null(ticket);
        Assert.Contains("Tipe tiket tidak ditemukan", error);
        Assert.Equal(3, await StockOfAsync(foreignType.EventTicketTypeId));
    }
}
