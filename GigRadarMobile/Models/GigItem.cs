namespace GigRadarMobile.Models;

/// <summary>
/// Gig (slot artist) pada event milik EO.
/// Sumber: GET /api/events/{eventId}/gigs (EO) dan GET /api/gigs (Discover artist).
/// </summary>
public class GigItem
{
    public int GigId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int? VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int RequiredArtistCount { get; set; }
    public string Status { get; set; } = "OPEN";
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventStartDate { get; set; }
    public int ApplicantCount { get; set; }
    public bool HasApplied { get; set; }

    public string ScheduleLabel =>
        $"{StartAt.ToLocalTime():dd MMM yyyy · HH:mm}–{EndAt.ToLocalTime():HH:mm}";
    public string BudgetLabel => BudgetMin == BudgetMax
        ? $"Rp {BudgetMin:N0}"
        : $"Rp {BudgetMin:N0}–{BudgetMax:N0}";
    public string StatusLabel => Status switch
    {
        "DRAFT" => "Draf",
        "CLOSED" => "Ditutup",
        _ => "Terbuka"
    };
    public string LocationLabel => string.Join(" • ",
        new[] { VenueName, City }.Where(value => !string.IsNullOrWhiteSpace(value)));
    public string ApplicantLabel => $"{ApplicantCount} pelamar · {RequiredArtistCount} slot";
    public bool IsOpen => Status == "OPEN";
    public bool IsDraft => Status == "DRAFT";
    /// <summary>Artist hanya bisa melamar gig yang terbuka dan belum pernah dilamar.</summary>
    public bool CanApply => IsOpen && !HasApplied;
    public string ApplyLabel => HasApplied ? "Sudah melamar" : IsOpen ? "Lamar gig" : "Ditutup";
    /// <summary>EO hanya bisa mengubah status lewat transisi DRAFT ⇄ OPEN → CLOSED.</summary>
    public string StatusActionLabel => IsOpen ? "Tutup lamaran" : "Buka lamaran";
    public Color StatusColor => Status switch
    {
        "DRAFT" => Color.FromArgb("#F59E0B"),
        "CLOSED" => Color.FromArgb("#71717A"),
        _ => Color.FromArgb("#22C55E")
    };
}
