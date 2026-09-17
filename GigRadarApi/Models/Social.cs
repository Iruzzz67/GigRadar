using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    /// <summary>
    /// Like user pada post artist (GIGRADAR_APP_SPEC.md Bab 32 — Social System).
    /// Satu user maksimal satu like per post (toggle like/unlike di controller).
    /// </summary>
    public class PostLike
    {
        [Key]
        public int PostLikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PostId")]
        [JsonIgnore]
        public ArtistPost? Post { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }

    /// <summary>
    /// Komentar user pada post artist (GIGRADAR_APP_SPEC.md Bab 32, 45).
    /// Admin dapat menghapus komentar saat moderasi (Bab 37).
    /// </summary>
    public class PostComment
    {
        [Key]
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PostId")]
        [JsonIgnore]
        public ArtistPost? Post { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
