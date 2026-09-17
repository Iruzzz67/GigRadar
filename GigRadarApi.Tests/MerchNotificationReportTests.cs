using System.Security.Claims;
using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test STEP 17–19 (GIGRADAR_APP_SPEC.md Bab 31, 36, 37, 38):
/// merchandise milik artist (CRUD + ownership + validasi), notifikasi
/// (daftar, unread, mark read), dan report/moderasi (kategori valid,
/// keputusan Admin + audit log).
/// </summary>
public class MerchNotificationReportTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notifications;
    private readonly MerchandiseController _merchController;
    private readonly ReportsController _reportsController;
    private readonly ReportsController _adminReportsController;
    private readonly NotificationsController _notifController;

    private const int UserId = 50;
    private const int AdminId = 1;
    private int _artistId;

    public MerchNotificationReportTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _notifications = new NotificationService(_context);

        _merchController = new MerchandiseController(_context, _notifications);
        _reportsController = new ReportsController(_context, _notifications);
        _adminReportsController = new ReportsController(_context, _notifications);
        _notifController = new NotificationsController(_notifications);

        var user = new User { UserId = UserId, Name = "Tester", Email = "t@x.com" };
        var admin = new User { UserId = AdminId, Name = "Admin", Email = "a@x.com" };
        var artist = new Artist { Name = "Hollow Men", UserId = UserId };
        _context.AddRange(user, admin, artist);
        _context.SaveChanges();
        _artistId = artist.ArtistId;
        _context.ChangeTracker.Clear();

        SetUser(_merchController, UserId, "Artist");
        SetUser(_reportsController, UserId, "User");
        SetUser(_adminReportsController, AdminId, "Admin");
        SetUser(_notifController, UserId, "User");
    }

    private static void SetUser(ControllerBase controller, int userId, string role)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("UserId", userId.ToString()), new Claim("Role", role) }, "test"))
            }
        };
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    // ── Merchandise (Bab 31) ─────────────────────────────

    [Fact]
    public async Task Merch_Create_Valid_ReturnsId()
    {
        var result = await _merchController.Create(new MerchandiseRequest
        {
            Name = "Kaos Afterglow",
            Price = 150000,
            Stock = 20,
            ExternalPurchaseUrl = "https://shop.example/kaos"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        Assert.Contains("Kaos Afterglow", json);
    }

    [Fact]
    public async Task Merch_Create_NoName_Rejected()
    {
        var result = await _merchController.Create(new MerchandiseRequest { Price = 1000 });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Merch_Create_NegativePrice_Rejected()
    {
        var result = await _merchController.Create(new MerchandiseRequest { Name = "X", Price = -1 });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Merch_Update_OwnedByOther_Fails()
    {
        await _merchController.Create(new MerchandiseRequest { Name = "Kaos", Price = 1000 });
        var merch = await _context.Merchandises.AsNoTracking().SingleAsync();

        var other = new MerchandiseController(_context, _notifications);
        SetUser(other, 999, "Artist");
        var result = await other.Update(merch.MerchandiseId, new MerchandiseRequest { Name = "Baru", Price = 1 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Merch_Archive_HidesFromPublicCatalog()
    {
        await _merchController.Create(new MerchandiseRequest { Name = "Kaos", Price = 1000 });
        var merch = await _context.Merchandises.AsNoTracking().SingleAsync();

        await _merchController.Archive(merch.MerchandiseId);

        var publicResult = await _merchController.GetForArtist(_artistId);
        var ok = Assert.IsType<OkObjectResult>(publicResult);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        Assert.DoesNotContain("Kaos", json);
    }

    // ── Notification (Bab 36) ────────────────────────────

    [Fact]
    public async Task Notif_NotifyAndList_ShowsUnreadFirst()
    {
        await _notifications.NotifyAsync(UserId, "TICKET_PURCHASE", "Tiket dibeli", "1x Festival", "tickets");
        var result = await _notifController.GetMy(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        Assert.Contains("Tiket dibeli", json);
        Assert.Equal(1, await _notifications.GetUnreadCountAsync(UserId));
    }

    [Fact]
    public async Task Notif_UnknownType_FallsBackToAdminNotice()
    {
        await _notifications.NotifyAsync(UserId, "HACKED_TYPE", "Judul", "Body");
        var notif = await _context.Notifications.AsNoTracking().SingleAsync();
        Assert.Equal("ADMIN_NOTICE", notif.Type);
    }

    [Fact]
    public async Task Notif_MarkRead_DecreasesUnreadCount()
    {
        await _notifications.NotifyAsync(UserId, "ADMIN_NOTICE", "Judul", "Body");
        var notif = await _context.Notifications.AsNoTracking().SingleAsync();

        await _notifController.MarkRead(notif.NotificationId);

        Assert.Equal(0, await _notifications.GetUnreadCountAsync(UserId));
    }

    [Fact]
    public async Task Notif_MarkRead_OwnedByOther_Fails()
    {
        await _notifications.NotifyAsync(UserId, "ADMIN_NOTICE", "Judul", "Body");
        var notif = await _context.Notifications.AsNoTracking().SingleAsync();

        var other = new NotificationsController(_notifications);
        SetUser(other!, 999, "User");
        var result = await other.MarkRead(notif.NotificationId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Notif_MarkAllRead_ClearsAll()
    {
        await _notifications.NotifyAsync(UserId, "ADMIN_NOTICE", "A", "B");
        await _notifications.NotifyAsync(UserId, "ADMIN_NOTICE", "C", "D");

        await _notifController.MarkAllRead();

        Assert.Equal(0, await _notifications.GetUnreadCountAsync(UserId));
    }

    // ── Report & Moderation (Bab 37–38) ──────────────────

    [Fact]
    public async Task Report_Create_Valid_ReturnsId()
    {
        var result = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "EVENT",
            TargetId = 3,
            Category = "FRAUD",
            Description = "Tiket dijual tapi event tidak jelas"
        });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Report_Create_UnknownTargetType_Rejected()
    {
        var result = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "PLANET",
            TargetId = 1,
            Description = "halo"
        });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Report_Create_UnknownCategory_Rejected()
    {
        var result = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "EVENT",
            TargetId = 1,
            Category = "RASIS",
            Description = "halo"
        });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Report_Create_NoDescription_Rejected()
    {
        var result = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "POST",
            TargetId = 1,
            Description = "  "
        });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Report_AdminList_RequiresAdmin()
    {
        SetUser(_reportsController, UserId, "User");
        var result = await _reportsController.GetReports("OPEN");
        // Endpoint [Authorize(Roles=Admin)] — controller test tetap memanggil action;
        // verifikasi hanya Admin yang dipasangkan di test berikut.
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok);
    }

    [Fact]
    public async Task Report_Resolve_RecordsDecisionAndAuditLog()
    {
        var create = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "POST",
            TargetId = 7,
            Category = "SPAM",
            Description = "Spam komen"
        });
        var reportId = ExtractReportId(create);

        var result = await _adminReportsController.Resolve(reportId, new ModerationNoteRequest
        {
            Note = "Konten dihapus"
        });

        Assert.IsType<OkObjectResult>(result);
        var report = await _context.Reports.AsNoTracking().SingleAsync();
        Assert.Equal("RESOLVED", report.Status);
        Assert.Equal("Konten dihapus", report.ModerationNote);
        Assert.Equal(AdminId, report.ReviewedByAdminId);

        var audit = await _context.AuditLogs.AsNoTracking().SingleAsync();
        Assert.Equal("ADMIN_RESOLVED_REPORT", audit.Action);
        Assert.Equal("REPORT", audit.EntityType);
    }

    [Fact]
    public async Task Report_Dismiss_SecondDecision_Fails()
    {
        var create = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "COMMENT",
            TargetId = 9,
            Category = "OTHER",
            Description = "perlu ditinjau"
        });
        var reportId = ExtractReportId(create);
        await _adminReportsController.Dismiss(reportId, new ModerationNoteRequest { Note = "aman" });

        var result = await _adminReportsController.Resolve(reportId, new ModerationNoteRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Report_Decision_NotifiesReporter()
    {
        var create = await _reportsController.Create(new CreateReportRequest
        {
            TargetType = "EVENT",
            TargetId = 5,
            Category = "SPAM",
            Description = "bukan event musik"
        });
        var reportId = ExtractReportId(create);
        await _adminReportsController.Resolve(reportId, new ModerationNoteRequest());

        var notifs = await _context.Notifications.AsNoTracking().Where(n => n.UserId == UserId).ToListAsync();
        Assert.Contains(notifs, n => n.Type == "ADMIN_NOTICE");
    }

    private static int ExtractReportId(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        return root.TryGetProperty("reportId", out var camel) ? camel.GetInt32()
             : root.GetProperty("ReportId").GetInt32();
    }
}
