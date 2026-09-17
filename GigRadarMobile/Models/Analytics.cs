namespace GigRadarMobile.Models;

/// <summary>Statistik funnel satu event milik EO (Bab 69 — EO).</summary>
public class EoEventAnalytics
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public int RemainingStock { get; set; }
    public decimal Revenue { get; set; }
    public int TicketTypeCount { get; set; }
    public int LineupCount { get; set; }
    public int FavoritedCount { get; set; }
    public double ConversionRate { get; set; }

    public string DateLabel => StartDate.ToLocalTime().ToString("dd MMM yyyy");
    public string RevenueFormatted => $"Rp {Revenue:N0}";
    public string ConversionLabel => $"{ConversionRate:P0}";
    public string SoldLabel => $"{TicketsSold}/{Capacity} tiket";
    /// <summary>Progres bar konversi (0–1) — dibatasi agar tidak overflow.</summary>
    public double ConversionProgress => Math.Clamp(ConversionRate, 0, 1);

    // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted).
    public Color StatusColor => Status switch
    {
        "SoldOut" => Color.FromArgb("#F43F5E"),
        "Completed" => Color.FromArgb("#71717A"),
        "Draft" => Color.FromArgb("#F59E0B"),
        _ => Color.FromArgb("#22C55E")
    };
}

/// <summary>Statistik platform untuk Admin (Bab 69 — Admin).</summary>
public class PlatformAnalytics
{
    public int TotalUsers { get; set; }
    public int TotalArtists { get; set; }
    public int TotalEos { get; set; }
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int TotalTickets { get; set; }
    public int PendingArtistVerifications { get; set; }
    public int PendingEoVerifications { get; set; }
    public int OpenReports { get; set; }
    public int TotalRevenue { get; set; }

    public string TotalRevenueFormatted => $"Rp {TotalRevenue:N0}";
    public string PendingVerificationsLabel => $"{PendingArtistVerifications + PendingEoVerifications}";
    public int PendingVerificationsTotal => PendingArtistVerifications + PendingEoVerifications;
}
