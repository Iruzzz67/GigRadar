using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    /// <summary>
    /// Profil organizer / EO (GIGRADAR_APP_SPEC.md Bab 12, 42). UserId menghubungkan
    /// profil ini ke akun User ber-role EO — satu user maksimal satu profil organizer.
    /// </summary>
    public class Organizer
    {
        [Key]
        public int OrganizerId { get; set; }
        public int? UserId { get; set; }

        public string OrganizationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string SocialLinks { get; set; } = string.Empty;
        public int? FoundedYear { get; set; }

        // Verifikasi EO (GIGRADAR_APP_SPEC.md Bab 15):
        // NOT_SUBMITTED | PENDING | VERIFIED | REJECTED — hanya diubah oleh Admin
        // melalui OrganizerVerificationService, tidak pernah dari input client.
        public string VerificationStatus { get; set; } = "NOT_SUBMITTED";
        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public List<OrganizerPortfolioItem> PortfolioItems { get; set; } = new();
    }

    /// <summary>
    /// Item portofolio EO (GIGRADAR_APP_SPEC.md Bab 14) — bukti pengalaman
    /// menggelar event sebelumnya: nama event, tanggal, venue, deskripsi, poster,
    /// dokumentasi foto/link. Terkait verification sebagai bukti pendukung.
    /// </summary>
    public class OrganizerPortfolioItem
    {
        [Key]
        public int PortfolioItemId { get; set; }
        public int OrganizerId { get; set; }

        public string EventName { get; set; } = string.Empty;
        public DateTime? EventDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string PhotosUrl { get; set; } = string.Empty;
        public string DocumentationLink { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrganizerId")]
        [JsonIgnore]
        public Organizer? Organizer { get; set; }
    }

    /// <summary>
    /// Pengajuan verifikasi EO (GIGRADAR_APP_SPEC.md Bab 13–15). Snapshot data
    /// organizer saat submit + contact person; keputusan dijurnal di entitas ini
    /// (ReviewedByUserId, RejectedReason, ReviewedAt) sebagai jejak audit.
    /// Satu PENDING per organizer; yang ditolak boleh diajukan ulang.
    /// </summary>
    public class OrganizerVerification
    {
        [Key]
        public int VerificationId { get; set; }
        public int OrganizerId { get; set; }

        // Snapshot profil saat submit (Bab 13: Organizer Profile → Portfolio → Submit)
        public string OrganizationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;

        /// <summary>Jumlah item portofolio saat pengajuan (bukti pengalaman).</summary>
        public int PortfolioCount { get; set; }
        /// <summary>Ringkasan link dokumentasi portofolio saat pengajuan.</summary>
        public string PortfolioSummary { get; set; } = string.Empty;

        /// <summary>PENDING | VERIFIED | REJECTED — pengajuan baru selalu PENDING.</summary>
        public string Status { get; set; } = "PENDING";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        public string RejectedReason { get; set; } = string.Empty;

        [ForeignKey("OrganizerId")]
        [JsonIgnore]
        public Organizer? Organizer { get; set; }
    }
}
