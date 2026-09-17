namespace GigRadarMobile.Models;

/// <summary>Komentar pada post artist (Bab 32).</summary>
public class PostCommentItem
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhotoUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd MMM HH:mm");
    public string Initials
    {
        get
        {
            var parts = UserName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }
}

/// <summary>Notifikasi user (Bab 36).</summary>
public class NotificationItem
{
    public int NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd MMM, HH:mm");
    public bool IsUnread => !IsRead;

    /// <summary>Ikon per tipe notifikasi (Bab 36).</summary>
    public string TypeIcon => Type switch
    {
        "ARTIST_INVITATION" => "🎤",
        "ARTIST_VERIFICATION" or "EO_VERIFICATION" => "🛡️",
        "TICKET_PURCHASE" or "TICKET_UPDATE" => "🎟️",
        "EVENT_CANCELLED" => "🚫",
        "ARTIST_NEW_POST" or "ARTIST_NEW_RELEASE" => "🎸",
        "ADMIN_NOTICE" => "📢",
        _ => "🔔"
    };
}

/// <summary>Envelope GET /api/notifications/unread-count.</summary>
public class UnreadCountResponse
{
    public int Count { get; set; }
}

/// <summary>Undangan event untuk artist/EO (Bab 25–26).</summary>
public class InvitationItem
{
    public int InvitationId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd MMM yyyy");
    public string EventDateDisplay => EventDate?.ToLocalTime().ToString("dd MMM yyyy") ?? "";
    public bool IsPending => Status == "PENDING";

    public string StatusDisplay => Status switch
    {
        "PENDING" => "Menunggu jawaban",
        "ACCEPTED" => "Diterima",
        "REJECTED" => "Ditolak",
        "CANCELLED" => "Dibatalkan",
        _ => Status
    };

    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted).
    public string StatusColor => Status switch
    {
        "ACCEPTED" => "#22C55E",
        "PENDING" => "#F59E0B",
        "REJECTED" => "#F43F5E",
        _ => "#71717A"
    };
}

/// <summary>Envelope GET /api/invitations/me.</summary>
public class InvitationsResponse
{
    public List<InvitationItem> Received { get; set; } = new();
    public List<InvitationItem> Sent { get; set; } = new();
}

/// <summary>Produk merchandise artist (Bab 31).</summary>
public class MerchItem
{
    public int MerchandiseId { get; set; }
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ExternalPurchaseUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "ACTIVE";

    public string PriceFormatted => $"Rp {Price:N0}";
    public string StockLabel => Stock > 0 ? $"Stok {Stock}" : "Stok habis";
    public bool HasPurchaseLink => !string.IsNullOrWhiteSpace(ExternalPurchaseUrl);
    public bool HasStock => Stock > 0;
}

/// <summary>Baris laporan untuk Admin (Bab 37).</summary>
public class ReportItem
{
    public int ReportId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd MMM yyyy");
    public string TargetLabel => $"{TargetType}#{TargetId}";

    public string CategoryDisplay => Category switch
    {
        "SPAM" => "Spam",
        "IMPERSONATION" => "Penyrupaan",
        "FRAUD" => "Penipuan",
        "INAPPROPRIATE_CONTENT" => "Konten tidak pantas",
        "MISLEADING_INFORMATION" => "Informasi menyesatkan",
        _ => "Lainnya"
    };
}
