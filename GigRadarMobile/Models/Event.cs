namespace GigRadarMobile.Models
{
    public class GigEvent
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string TicketLink { get; set; } = string.Empty;
        public int? VenueId { get; set; }
        public Venue? Venue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? GenreId { get; set; }
        public Genre? Genre { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Capacity { get; set; }
        public int ViewsCount { get; set; }
        public int SavesCount { get; set; }
        public string Status { get; set; } = "Published";
        public List<EventArtist> EventArtists { get; set; } = new();

        public string StatusLabel => Status switch
        {
            "SoldOut" => "Tiket Habis",
            "Completed" => "Event Selesai",
            "Draft" => "Draft",
            _ => "Aktif"
        };

        public Color StatusColor => Status switch
        {
            "SoldOut" => Color.FromArgb("#FF6B6B"),
            "Completed" => Color.FromArgb("#9E9E9E"),
            "Draft" => Color.FromArgb("#FFB020"),
            _ => Color.FromArgb("#39FF14")
        };

        public bool ShowsStatusBadge => Status != "Published";
        public bool IsBuyable => Status == "Published";

        public string DateFormatted => StartDate.ToString("dd MMM yyyy");
        public string TimeFormatted => StartDate.ToString("HH:mm");
        public string PriceFormatted => MinPrice == MaxPrice
            ? $"Rp {MinPrice:N0}"
            : $"Rp {MinPrice:N0} - Rp {MaxPrice:N0}";
        public string LineupNames => string.Join(", ", EventArtists.Select(ea => ea.Artist?.Name ?? ""));
        public string VenueName => Venue?.Name ?? "TBA";
        public bool HasExternalLink => !string.IsNullOrWhiteSpace(TicketLink);
    }

    public class EventArtist
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ArtistId { get; set; }
        public int Order { get; set; }
        public Artist? Artist { get; set; }
    }
}
