using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        [JsonIgnore]
        public Artist? Artist { get; set; }
    }
}
