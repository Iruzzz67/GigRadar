using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Capacity { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Event> Events { get; set; } = new();
    }
}
