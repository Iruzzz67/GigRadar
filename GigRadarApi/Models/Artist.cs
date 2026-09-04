using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    /// <summary>
    /// Profil artist (GIGRADAR_ROLE_SYSTEM.md §6/§8). UserId menghubungkan profil
    /// ini ke akun User ber-role Artist — satu user maksimal satu profil artist.
    /// </summary>
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string SocialLinks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public List<EventArtist> EventArtists { get; set; } = new();

        [JsonIgnore]
        public List<Follow> Followers { get; set; } = new();

        [JsonIgnore]
        public List<AudioTrack> Tracks { get; set; } = new();

        [JsonIgnore]
        public List<ArtistAlbum> Albums { get; set; } = new();

        [JsonIgnore]
        public List<ArtistPost> Posts { get; set; } = new();

        [JsonIgnore]
        public List<ArtistMember> Members { get; set; } = new();

        [JsonIgnore]
        public List<ArtistJourneyItem> Journey { get; set; } = new();
    }

    /// <summary>Lagu / rilis single artist (§9).</summary>
    public class AudioTrack
    {
        [Key]
        public int TrackId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int DurationSeconds { get; set; } = 30;
        public DateTime? ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }

    /// <summary>Album artist (§9 — Music → Albums).</summary>
    public class ArtistAlbum
    {
        [Key]
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }

    /// <summary>Post artist — update band, poster, pengumuman, dll (§10).</summary>
    public class ArtistPost
    {
        [Key]
        public int PostId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }

    /// <summary>Anggota band (§8 — band members).</summary>
    public class ArtistMember
    {
        [Key]
        public int MemberId { get; set; }
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }

    /// <summary>
    /// Perjalanan perkembangan artist (§11) — kategori Formation/Release/Gig/
    /// Achievement/Album/Single/Other.
    /// </summary>
    public class ArtistJourneyItem
    {
        [Key]
        public int JourneyId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = "Other";
        public DateTime? Date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ArtistId")]
        public Artist? Artist { get; set; }
    }
}