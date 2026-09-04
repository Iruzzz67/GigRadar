namespace GigRadarMobile.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string TicketType { get; set; } = "Regular";
        public decimal Price { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public DateTime? BuyerDateOfBirth { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime PurchasedAt { get; set; }
        public GigEvent? Event { get; set; }

        public string PriceFormatted => $"Rp {Price:N0}";
        public string EventName => Event?.Name ?? "Unknown Event";
        public string EventDate => Event?.DateFormatted ?? "";
        public string VenueName => Event?.VenueName ?? "TBA";
        public string PurchasedFormatted => PurchasedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm");
        public string QrDisplay => QRCode.Length > 24 ? QRCode[..24] + "..." : QRCode;
    }
}