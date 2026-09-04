using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<UserPreference> Preferences { get; set; } = new();

        [JsonIgnore]
        public List<Ticket> Tickets { get; set; } = new();

        [JsonIgnore]
        public List<Favorite> Favorites { get; set; } = new();

        [JsonIgnore]
        public List<Follow> Following { get; set; } = new();
    }

    public class UserPreference
    {
        [Key]
        public int PreferenceId { get; set; }
        public int UserId { get; set; }
        public int GenreId { get; set; }
        public double Weight { get; set; } = 1.0;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("GenreId")]
        public Genre? Genre { get; set; }
    }

    public class Follow
    {
        [Key]
        public int FollowId { get; set; }
        public int UserId { get; set; }
        public int? ArtistId { get; set; }
        public int? VenueId { get; set; }
        public int? EventOrganizerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }
    }
}
