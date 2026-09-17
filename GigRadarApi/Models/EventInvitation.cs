using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    /// <summary>
    /// Undangan artist ke event oleh EO (GIGRADAR_APP_SPEC.md Bab 25–26).
    /// Status: PENDING | ACCEPTED | REJECTED | CANCELLED.
    /// Saat ACCEPTED, artist masuk line-up event (EventArtist dibuat otomatis).
    /// Hanya satu undangan aktif (PENDING) per pasangan event-artist.
    /// </summary>
    public class EventInvitation
    {
        [Key]
        public int InvitationId { get; set; }
        public int EventId { get; set; }
        public int ArtistId { get; set; }

        /// <summary>EO pembuat undangan (jejak audit).</summary>
        public int CreatedByUserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        [ForeignKey("EventId")]
        [JsonIgnore]
        public Event? Event { get; set; }

        [ForeignKey("ArtistId")]
        [JsonIgnore]
        public Artist? Artist { get; set; }
    }
}
