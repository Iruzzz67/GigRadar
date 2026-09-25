using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class CommunityPost
    {
        [Key]
        public int CommunityPostId { get; set; }
        public int ArtistId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        [JsonIgnore]
        public Artist? Artist { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
