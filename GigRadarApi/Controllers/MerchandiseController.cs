using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;

namespace GigRadarApi.Controllers
{
    /// <summary>Body POST /api/artists/me/merchandise (Bab 31).</summary>
    public class MerchandiseRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ExternalPurchaseUrl { get; set; }
    }

    /// <summary>Body POST /api/reports (Bab 37).</summary>
    public class CreateReportRequest
    {
        public string? TargetType { get; set; }
        public int TargetId { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Merchandise artist (GIGRADAR_APP_SPEC.md Bab 31, 48): CRUD milik artist,
    /// MVP memakai link pembelian eksternal. Endpoint publik untuk melihat katalog.
    /// </summary>
    [Route("api")]
    [ApiController]
    [Authorize]
    public class MerchandiseController(AppDbContext context, NotificationService notifications) : ControllerBase
    {
        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>Katalog merchandise artist (publik bagi user terautentikasi).</summary>
        [HttpGet("artists/{artistId}/merchandise")]
        public async Task<IActionResult> GetForArtist(int artistId)
        {
            var items = await context.Merchandises
                .Where(m => m.ArtistId == artistId && m.Status == "ACTIVE")
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Daftar merchandise milik artist yang login (termasuk yang diarsipkan).</summary>
        [HttpGet("artists/me/merchandise")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetMine()
        {
            var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == CurrentUserId);
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var items = await context.Merchandises
                .Where(m => m.ArtistId == artist.ArtistId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(items);
        }

        /// <summary>Tambah produk merchandise (Bab 31 — nama & harga wajib).</summary>
        [HttpPost("artists/me/merchandise")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Create([FromBody] MerchandiseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Nama produk wajib diisi" });
            if (request.Price < 0)
                return BadRequest(new { message = "Harga tidak boleh negatif" });

            var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == CurrentUserId);
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var merch = new Merchandise
            {
                ArtistId = artist.ArtistId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? "",
                ImageUrl = request.ImageUrl?.Trim() ?? "",
                Price = request.Price,
                Stock = Math.Max(0, request.Stock),
                ExternalPurchaseUrl = request.ExternalPurchaseUrl?.Trim() ?? ""
            };
            context.Merchandises.Add(merch);
            await context.SaveChangesAsync();

            // Notifikasi pengikut artist (Bab 36).
            var followerIds = await context.Follows
                .Where(f => f.ArtistId == artist.ArtistId)
                .Select(f => f.UserId)
                .ToListAsync();
            foreach (var fid in followerIds)
            {
                await notifications.NotifyAsync(fid, "ARTIST_NEW_POST",
                    $"{artist.Name} rilis merch baru",
                    $"{merch.Name} sudah tersedia — cek halaman artist!",
                    $"artist/{artist.ArtistId}");
            }

            return Ok(new { message = $"Merchandise \"{merch.Name}\" ditambahkan", merch.MerchandiseId });
        }

        /// <summary>Perbarui produk milik sendiri.</summary>
        [HttpPut("merchandise/{merchandiseId}")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Update(int merchandiseId, [FromBody] MerchandiseRequest request)
        {
            var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == CurrentUserId);
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var merch = await context.Merchandises
                .FirstOrDefaultAsync(m => m.MerchandiseId == merchandiseId && m.ArtistId == artist.ArtistId);
            if (merch == null) return NotFound(new { message = "Merchandise tidak ditemukan" });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Nama produk wajib diisi" });

            merch.Name = request.Name.Trim();
            merch.Description = request.Description?.Trim() ?? "";
            merch.ImageUrl = request.ImageUrl?.Trim() ?? "";
            merch.Price = request.Price;
            merch.Stock = Math.Max(0, request.Stock);
            merch.ExternalPurchaseUrl = request.ExternalPurchaseUrl?.Trim() ?? "";
            await context.SaveChangesAsync();
            return Ok(new { message = "Merchandise diperbarui" });
        }

        /// <summary>Arsipkan produk (soft-delete — riwayat tetap utuh).</summary>
        [HttpDelete("merchandise/{merchandiseId}")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Archive(int merchandiseId)
        {
            var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == CurrentUserId);
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var merch = await context.Merchandises
                .FirstOrDefaultAsync(m => m.MerchandiseId == merchandiseId && m.ArtistId == artist.ArtistId);
            if (merch == null) return NotFound(new { message = "Merchandise tidak ditemukan" });

            merch.Status = "ARCHIVED";
            await context.SaveChangesAsync();
            return Ok(new { message = "Merchandise diarsipkan" });
        }
    }

    /// <summary>
    /// Notifikasi user (GIGRADAR_APP_SPEC.md Bab 36, 48): daftar, badge unread,
    /// tandai dibaca satu/semua.
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController(NotificationService service) : ControllerBase
    {
        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>Daftar notifikasi user (?unread=true untuk yang belum dibaca).</summary>
        [HttpGet]
        public async Task<IActionResult> GetMy([FromQuery] string? unread)
        {
            var items = await service.GetMyAsync(CurrentUserId, unreadOnly: unread == "true");
            return Ok(items);
        }

        /// <summary>Jumlah notifikasi belum dibaca (badge).</summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            return Ok(new { count = await service.GetUnreadCountAsync(CurrentUserId) });
        }

        /// <summary>Tandai satu notifikasi dibaca.</summary>
        [HttpPost("{notificationId}/read")]
        public async Task<IActionResult> MarkRead(int notificationId)
        {
            var (success, message) = await service.MarkReadAsync(CurrentUserId, notificationId);
            if (!success) return NotFound(new { message });
            return Ok(new { message });
        }

        /// <summary>Tandai semua notifikasi dibaca.</summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var count = await service.MarkAllReadAsync(CurrentUserId);
            return Ok(new { message = $"{count} notifikasi ditandai dibaca" });
        }
    }

    /// <summary>
    /// Reporting &amp; moderation (GIGRADAR_APP_SPEC.md Bab 37, 49):
    /// user melaporkan konten/akun; Admin resolve/dismiss + catatan moderasi;
    /// aksi penting Admin dicatat ke audit log (Bab 38).
    /// </summary>
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReportsController(AppDbContext context, NotificationService notifications) : ControllerBase
    {
        private static readonly string[] AllowedTargets = ["ARTIST", "EO", "EVENT", "POST", "COMMENT", "MERCHANDISE"];
        private static readonly string[] AllowedCategories =
            ["SPAM", "IMPERSONATION", "FRAUD", "INAPPROPRIATE_CONTENT", "MISLEADING_INFORMATION", "OTHER"];

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>User membuat laporan (Bab 37).</summary>
        [HttpPost("reports")]
        public async Task<IActionResult> Create([FromBody] CreateReportRequest request)
        {
            var targetType = request.TargetType?.Trim().ToUpperInvariant() ?? "";
            if (!AllowedTargets.Contains(targetType))
                return BadRequest(new { message = "Jenis laporan tidak dikenal" });

            var category = request.Category?.Trim().ToUpperInvariant() ?? "OTHER";
            if (!AllowedCategories.Contains(category))
                return BadRequest(new { message = "Kategori laporan tidak dikenal" });

            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Deskripsi laporan wajib diisi" });

            var report = new Report
            {
                ReporterUserId = CurrentUserId,
                TargetType = targetType,
                TargetId = request.TargetId,
                Category = category,
                Description = request.Description.Trim()
            };
            context.Reports.Add(report);
            await context.SaveChangesAsync();
            return Ok(new { message = "Laporan terkirim — akan ditinjau Admin", report.ReportId });
        }

        /// <summary>Daftar laporan untuk Admin (Bab 49; ?status=OPEN default).</summary>
        [HttpGet("admin/reports")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReports([FromQuery] string? status = "OPEN")
        {
            var query = context.Reports.AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);

            var reports = await query
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReportId,
                    r.ReporterUserId,
                    reporterName = r.Reporter!.Name,
                    r.TargetType,
                    r.TargetId,
                    r.Category,
                    r.Description,
                    r.Status,
                    r.ModerationNote,
                    r.CreatedAt
                })
                .ToListAsync();
            return Ok(reports);
        }

        /// <summary>Admin menyelesaikan laporan (resolve) + catatan moderasi + audit log.</summary>
        [HttpPost("admin/reports/{reportId}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(int reportId, [FromBody] ModerationNoteRequest request)
        {
            return await DecideAsync(reportId, "RESOLVED", request);
        }

        /// <summary>Admin menolak laporan (dismiss) + audit log.</summary>
        [HttpPost("admin/reports/{reportId}/dismiss")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dismiss(int reportId, [FromBody] ModerationNoteRequest request)
        {
            return await DecideAsync(reportId, "DISMISSED", request);
        }

        private async Task<IActionResult> DecideAsync(int reportId, string decision, ModerationNoteRequest? request)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var report = await context.Reports.FindAsync(reportId);
            if (report == null) return NotFound(new { message = "Laporan tidak ditemukan" });

            if (report.Status != "OPEN")
                return BadRequest(new { message = "Laporan ini sudah diputuskan" });

            report.Status = decision;
            report.ModerationNote = request?.Note?.Trim() ?? "";
            report.ReviewedByAdminId = adminId;
            report.ReviewedAt = DateTime.UtcNow;

            // Audit log (Bab 38) — jejak keputusan moderasi.
            context.AuditLogs.Add(new AuditLog
            {
                AdminUserId = adminId,
                Action = decision == "RESOLVED" ? "ADMIN_RESOLVED_REPORT" : "ADMIN_DISMISSED_REPORT",
                EntityType = "REPORT",
                EntityId = reportId,
                Description = $"{report.TargetType}#{report.TargetId} — {report.Category}"
            });

            await context.SaveChangesAsync();

            // Kabari pelapor agar loop laporan tertutup.
            var reporter = await context.Users.FindAsync(report.ReporterUserId);
            if (reporter != null)
            {
                await notifications.NotifyAsync(reporter.UserId, "ADMIN_NOTICE",
                    "Laporanmu ditinjau",
                    decision == "RESOLVED"
                        ? "Laporanmu ditindaklanjuti. Terima kasih sudah menjaga platform."
                        : "Laporanmu ditinjau dan tidak melanggar aturan platform.",
                    "reports");
            }

            return Ok(new { message = decision == "RESOLVED" ? "Laporan diselesaikan" : "Laporan ditolak" });
        }
    }

    public class ModerationNoteRequest
    {
        public string? Note { get; set; }
    }
}
