namespace GigRadarMobile.Models;

/// <summary>Status verifikasi artist milik user yang login (GET /api/artist/verification/status).</summary>
public class ArtistVerificationStatus
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "NOT_SUBMITTED";
    public bool IsVerified { get; set; }
    public bool CanSubmit { get; set; }
    public VerificationHistoryItem? Latest { get; set; }

    public string StatusDisplay => VerificationStatus switch
    {
        "NOT_SUBMITTED" => "Belum mengajukan",
        "PENDING" => "Menunggu tinjauan",
        "VERIFIED" => "Terverifikasi",
        "REJECTED" => "Ditolak",
        _ => VerificationStatus
    };

    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted) — C# tidak bisa StaticResource.
    public string StatusColor => VerificationStatus switch
    {
        "VERIFIED" => "#22C55E",
        "PENDING" => "#F59E0B",
        "REJECTED" => "#F43F5E",
        _ => "#71717A"
    };
}

/// <summary>Pengajuan verifikasi terakhir milik artist (dari GET /api/artist/verification/status).</summary>
public class VerificationHistoryItem
{
    public int VerificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string RejectedReason { get; set; } = string.Empty;

    public string SubmittedAtDisplay => SubmittedAt.ToString("dd MMM yyyy");

    /// <summary>Alasan penolakan hanya relevan bila status REJECTED.</summary>
    public bool HasRejectedReason =>
        !string.IsNullOrWhiteSpace(RejectedReason) && Status == "REJECTED";
}

/// <summary>Baris pengajuan verifikasi di Verification Center Admin (GET /api/users/artist-verifications).</summary>
public class ArtistVerificationItem
{
    public int VerificationId { get; set; }
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string ArtistGenre { get; set; } = string.Empty;
    public string ArtistCity { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string SpotifyUrl { get; set; } = string.Empty;
    public string SupportingInfo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public string SubmittedAtDisplay => SubmittedAt.ToString("dd MMM yyyy");

    public bool HasSpotify => !string.IsNullOrWhiteSpace(SpotifyUrl);
    public bool HasSupportingInfo => !string.IsNullOrWhiteSpace(SupportingInfo);

    public string StatusDisplay => Status switch
    {
        "VERIFIED" => "Terverifikasi",
        "PENDING" => "Menunggu tinjauan",
        "REJECTED" => "Ditolak",
        _ => Status
    };

    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor).
    public string StatusColor => Status switch
    {
        "VERIFIED" => "#22C55E",
        "PENDING" => "#F59E0B",
        "REJECTED" => "#F43F5E",
        _ => "#71717A"
    };
}
