namespace GigRadarMobile.Models;

public class CreateGigData
{
    public string Name { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int RequiredArtistCount { get; set; } = 1;
    public string Status { get; set; } = "OPEN";
}
