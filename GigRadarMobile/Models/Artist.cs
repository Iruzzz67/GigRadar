namespace GigRadarMobile.Models
{
    /// <summary>Profil artist publik (discovery — GET /api/artists/{id}).</summary>
    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string SocialLinks { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int TracksCount { get; set; }
        public List<ArtistMember> Members { get; set; } = new();
        public List<AudioTrack> Tracks { get; set; } = new();
        public List<ArtistAlbum> Albums { get; set; } = new();
        public List<ArtistPost> Posts { get; set; } = new();
        public List<ArtistJourneyItem> Journey { get; set; } = new();
    }

    /// <summary>Profil artist milik user yang login (GET /api/artist/me).</summary>
    public class ArtistProfile
    {
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string SocialLinks { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int TracksCount { get; set; }
        public int AlbumsCount { get; set; }
        public int PostsCount { get; set; }
        public List<ArtistMember> Members { get; set; } = new();
    }

    public class AudioTrack
    {
        public int TrackId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int DurationSeconds { get; set; } = 30;
        public DateTime? ReleaseDate { get; set; }

        public string DurationLabel => DurationSeconds > 0 ? $"{DurationSeconds / 60}:{DurationSeconds % 60:00}" : "";
        public string ReleaseLabel => ReleaseDate?.ToString("dd MMM yyyy") ?? "Belum ada tanggal rilis";
    }

    public class ArtistAlbum
    {
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }

        public string ReleaseLabel => ReleaseDate?.ToString("dd MMM yyyy") ?? "Belum ada tanggal rilis";
    }

    public class ArtistPost
    {
        public int PostId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string DateLabel => CreatedAt.ToString("dd MMM yyyy");
        public string StatusLabel => IsPublished ? "Published" : "Draft";
        public Color StatusColor => IsPublished ? Color.FromArgb("#39FF14") : Color.FromArgb("#B0B0B0");
    }

    public class ArtistMember
    {
        public int MemberId { get; set; }
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
    }

    public class ArtistJourneyItem
    {
        public int JourneyId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = "Other";
        public DateTime? Date { get; set; }

        public string YearLabel => Date?.Year.ToString() ?? "—";
        public string DateLabel => Date?.ToString("dd MMM yyyy") ?? "—";
    }

    /// <summary>Statistik dashboard Artist (§7).</summary>
    public class ArtistDashboard
    {
        public int ArtistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int TracksCount { get; set; }
        public int AlbumsCount { get; set; }
        public int PostsCount { get; set; }
        public List<ArtistGig> UpcomingGigs { get; set; } = new();
    }

    /// <summary>Gig — event yang menampilkan artist ini di line-up (GET /api/artists/{id}/events).</summary>
    public class ArtistGig
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Published";
        public string VenueName { get; set; } = string.Empty;
        public string VenueCity { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public string DateLabel => StartDate.ToString("dd MMM yyyy • HH:mm");
        public string LocationLabel => string.IsNullOrWhiteSpace(VenueName) ? VenueCity : $"{VenueName} • {VenueCity}";
        public bool IsUpcoming => StartDate >= DateTime.Now;
        public string StatusLabel => Status switch
        {
            "SoldOut" => "Tiket Habis",
            "Completed" => "Selesai",
            "Draft" => "Draft",
            _ => "Published"
        };
        public Color StatusColor => Status switch
        {
            "SoldOut" => Color.FromArgb("#FFB020"),
            "Completed" => Color.FromArgb("#B0B0B0"),
            _ => Color.FromArgb("#39FF14")
        };
    }
}