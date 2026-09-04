namespace GigRadarMobile.Models
{
    public class EventTicketType
    {
        public int EventTicketTypeId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int SortOrder { get; set; }

        public string PriceFormatted => $"Rp {Price:N0}";
        public bool IsSoldOut => Stock <= 0;
        public string StockLabel => IsSoldOut ? "SOLD OUT" : $"Sisa {Stock} tiket";
        public string BadgeColor => IsSoldOut ? "#555555" : "#39FF14";
    }
}