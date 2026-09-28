using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models;

/// <summary>
/// Kesempatan tampil (slot artist) yang menempel pada event milik EO.
/// Status: DRAFT | OPEN | CLOSED — transisi divalidasi server-side di GigService.
/// </summary>
public class Gig
{
    [Key]
    public int GigId { get; set; }

    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int? VenueId { get; set; }
    public int? GenreId { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int RequiredArtistCount { get; set; } = 1;
    public string Status { get; set; } = "OPEN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(EventId))]
    [JsonIgnore]
    public Event? Event { get; set; }

    [ForeignKey(nameof(VenueId))]
    public Venue? Venue { get; set; }

    [ForeignKey(nameof(GenreId))]
    public Genre? Genre { get; set; }

    [JsonIgnore]
    public List<GigApplication> Applications { get; set; } = new();
}

/// <summary>
/// Lamaran artist untuk sebuah Gig (alur "Apply" — §5/§6 dokumen research).
/// Status: PENDING | SHORTLISTED | REJECTED | WITHDRAWN.
/// Satu artist maksimal satu lamaran per gig (unique index).
/// </summary>
public class GigApplication
{
    [Key]
    public int GigApplicationId { get; set; }

    public int GigId { get; set; }
    public int ArtistId { get; set; }

    /// <summary>Pesan perkenalan / penawaran singkat dari artist.</summary>
    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Waktu EO memutuskan (shortlist/reject) atau artist menarik lamaran.</summary>
    public DateTime? RespondedAt { get; set; }

    [ForeignKey(nameof(GigId))]
    [JsonIgnore]
    public Gig? Gig { get; set; }

    [ForeignKey(nameof(ArtistId))]
    public Artist? Artist { get; set; }
}
