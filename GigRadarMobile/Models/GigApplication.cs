namespace GigRadarMobile.Models;

/// <summary>
/// Lamaran artist untuk sebuah gig.
/// GET /api/gigs/{gigId}/applications (EO) dan GET /api/applications/me (Artist).
/// Status: PENDING | SHORTLISTED | REJECTED | WITHDRAWN.
/// </summary>
public class GigApplicationItem
{
    public int GigApplicationId { get; set; }
    public int GigId { get; set; }
    public int ArtistId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string ArtistGenre { get; set; } = string.Empty;
    public string ArtistCity { get; set; } = string.Empty;
    public string GigName { get; set; } = string.Empty;
    public int? EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    public string StatusLabel => Status switch
    {
        "PENDING" => "Menunggu",
        "SHORTLISTED" => "Shortlist",
        "REJECTED" => "Ditolak",
        "WITHDRAWN" => "Ditarik",
        _ => Status
    };
    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted).
    public Color StatusColor => Status switch
    {
        "SHORTLISTED" => Color.FromArgb("#A3FF12"),
        "PENDING" => Color.FromArgb("#F59E0B"),
        "REJECTED" => Color.FromArgb("#F43F5E"),
        _ => Color.FromArgb("#71717A")
    };
    public string AppliedLabel => $"Dilamar {CreatedAt.ToLocalTime():dd MMM yyyy}";
    public string ArtistLabel => string.Join(" • ",
        new[] { ArtistGenre, ArtistCity }.Where(value => !string.IsNullOrWhiteSpace(value)));
    public string TargetLabel => string.IsNullOrWhiteSpace(EventName)
        ? GigName
        : $"{GigName} · {EventName}";

    public bool CanWithdraw => Status is "PENDING" or "SHORTLISTED";
    public bool CanShortlist => Status == "PENDING";
    public bool CanReject => Status is "PENDING" or "SHORTLISTED";
}

/// <summary>Payload POST /api/gigs/{gigId}/applications.</summary>
public class GigApplicationData
{
    public string Message { get; set; } = string.Empty;
}
