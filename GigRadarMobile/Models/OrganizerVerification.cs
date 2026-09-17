namespace GigRadarMobile.Models;

/// <summary>Profil organizer / EO (GIGRADAR_APP_SPEC.md Bab 12, 42).</summary>
public class OrganizerProfile
{
    public int OrganizerId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string SocialLinks { get; set; } = string.Empty;
    public int? FoundedYear { get; set; }
    public string VerificationStatus { get; set; } = "NOT_SUBMITTED";
    public bool IsVerified { get; set; }

    public bool HasWebsite => !string.IsNullOrWhiteSpace(WebsiteUrl);
}

/// <summary>Item portofolio EO (Bab 14) — bukti pengalaman menggelar event.</summary>
public class PortfolioItem
{
    public int PortfolioItemId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotosUrl { get; set; } = string.Empty;
    public string DocumentationLink { get; set; } = string.Empty;

    public string EventDateDisplay => EventDate?.ToString("dd MMM yyyy") ?? "—";
    public bool HasLink => !string.IsNullOrWhiteSpace(DocumentationLink);
    public bool HasVenue => !string.IsNullOrWhiteSpace(Venue);
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}

/// <summary>Envelope GET /api/organizer/me.</summary>
public class OrganizerProfileResponse
{
    public OrganizerProfile Organizer { get; set; } = new();
    public List<PortfolioItem> Portfolio { get; set; } = new();
}

/// <summary>Status verifikasi EO milik user yang login (GET /api/organizer/verification/status).</summary>
public class OrganizerVerificationStatus
{
    public int OrganizerId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = "NOT_SUBMITTED";
    public bool IsVerified { get; set; }
    public int PortfolioCount { get; set; }
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

    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted).
    public string StatusColor => VerificationStatus switch
    {
        "VERIFIED" => "#22C55E",
        "PENDING" => "#F59E0B",
        "REJECTED" => "#F43F5E",
        _ => "#71717A"
    };
}

/// <summary>Baris pengajuan verifikasi EO di Verification Center Admin (GET /api/users/organizer-verifications).</summary>
public class OrganizerVerificationItem
{
    public int VerificationId { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public int PortfolioCount { get; set; }
    public string PortfolioSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public string SubmittedAtDisplay => SubmittedAt.ToString("dd MMM yyyy");
    public string PortfolioLabel => $"{PortfolioCount} portofolio";
    public bool HasWebsite => !string.IsNullOrWhiteSpace(WebsiteUrl);
    public bool HasPortfolioSummary => !string.IsNullOrWhiteSpace(PortfolioSummary);

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
