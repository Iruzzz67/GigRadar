namespace GigRadarMobile.Models
{
    /// <summary>
    /// Artist yang diikuti user login (GET /api/users/follows) — bahan halaman
    /// Following (GIGRADAR_APP_SPEC.md Bab 5/10).
    /// </summary>
    public class FollowedArtist
    {
        public int FollowId { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string ArtistPhotoUrl { get; set; } = string.Empty;
        public string ArtistGenre { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Initials => BuildInitials(ArtistName);

        public string GenreLabel =>
            string.IsNullOrWhiteSpace(ArtistGenre) ? "Genre belum diisi" : ArtistGenre;

        public string FollowedSinceLabel => CreatedAt == default
            ? "Diikuti"
            : $"Diikuti {CreatedAt.ToLocalTime():dd MMM yyyy}";

        private static string BuildInitials(string name)
        {
            var parts = (name ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }
}
