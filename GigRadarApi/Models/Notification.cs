using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    /// <summary>
    /// Notifikasi user (GIGRADAR_APP_SPEC.md Bab 36). Type mengikuti daftar resmi:
    /// NEW_GIG_NEARBY, FOLLOWED_ARTIST_NEW_GIG, ARTIST_NEW_RELEASE, ARTIST_NEW_POST,
    /// EVENT_REMINDER, TICKET_PURCHASE, TICKET_UPDATE, EVENT_CANCELLED,
    /// ARTIST_INVITATION, ARTIST_VERIFICATION, EO_VERIFICATION, ADMIN_NOTICE.
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = "ADMIN_NOTICE";
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        /// <summary>URL/teks navigasi opsional (mis. "event/3", "invitations").</summary>
        public string Link { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }

    /// <summary>
    /// Merchandise artist (GIGRADAR_APP_SPEC.md Bab 31). MVP: pembelian lewat
    /// ExternalPurchaseUrl (link eksternal), bukan marketplace penuh.
    /// </summary>
    public class Merchandise
    {
        [Key]
        public int MerchandiseId { get; set; }
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ExternalPurchaseUrl { get; set; } = string.Empty;
        /// <summary>ACTIVE | ARCHIVED.</summary>
        public string Status { get; set; } = "ACTIVE";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        [JsonIgnore]
        public Artist? Artist { get; set; }

        public bool HasPurchaseLink => !string.IsNullOrWhiteSpace(ExternalPurchaseUrl);
        public bool HasStock => Stock > 0;
    }

    /// <summary>
    /// Laporan konten/akun oleh user (GIGRADAR_APP_SPEC.md Bab 37).
    /// Category: SPAM | IMPERSONATION | FRAUD | INAPPROPRIATE_CONTENT |
    /// MISLEADING_INFORMATION | OTHER. Status: OPEN | RESOLVED | DISMISSED.
    /// </summary>
    public class Report
    {
        [Key]
        public int ReportId { get; set; }
        public int ReporterUserId { get; set; }

        /// <summary>ARTIST | EO | EVENT | POST | COMMENT | MERCHANDISE.</summary>
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string Category { get; set; } = "OTHER";
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "OPEN";
        public string ModerationNote { get; set; } = string.Empty;
        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ReporterUserId")]
        [JsonIgnore]
        public User? Reporter { get; set; }
    }

    /// <summary>
    /// Jejak audit aksi penting Admin (GIGRADAR_APP_SPEC.md Bab 38).
    /// Contoh action: ADMIN_APPROVED_ARTIST, ADMIN_REJECTED_EO, dst.
    /// Tidak pernah menyimpan password atau token.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }
        public int AdminUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
