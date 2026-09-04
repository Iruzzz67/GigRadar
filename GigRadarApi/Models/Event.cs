using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string TicketLink { get; set; } = string.Empty;
        public int? VenueId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? GenreId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Published";
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Capacity { get; set; }
        public int ViewsCount { get; set; }
        public int SavesCount { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [ForeignKey("GenreId")]
        public Genre? Genre { get; set; }

        [ForeignKey("CreatedBy")]
        public User? Organizer { get; set; }

        [JsonIgnore]
        public List<EventArtist> EventArtists { get; set; } = new();

        [JsonIgnore]
        public List<Ticket> Tickets { get; set; } = new();

        [JsonIgnore]
        public List<Favorite> Favorites { get; set; } = new();
    }

    public class EventArtist
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ArtistId { get; set; }
        public int Order { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }

    /// <summary>
    /// Tipe tiket yang tersedia untuk sebuah event (Festival, Tribun, Bundling, dll).
    /// Bila event memiliki TicketLink, pembelian dialihkan ke link eksternal tersebut.
    /// </summary>
    public class EventTicketType
    {
        [Key]
        public int EventTicketTypeId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int SortOrder { get; set; }

        [ForeignKey("EventId")]
        [JsonIgnore]
        public Event? Event { get; set; }
    }

    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
