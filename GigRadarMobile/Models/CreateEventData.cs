namespace GigRadarMobile.Models
{
    /// <summary>Payload pembuatan event baru oleh EO/Admin (POST /api/events).</summary>
    public class CreateEventData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string TicketLink { get; set; } = string.Empty;
        public int? VenueId { get; set; }
        public int? GenreId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = "Published";
    }
}
