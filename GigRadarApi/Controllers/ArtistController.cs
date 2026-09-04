using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    /// <summary>
    /// API manajemen Artist (GIGRADAR_ROLE_SYSTEM.md §32) — khusus role Artist.
    /// Semua aksi dibatasi ke profil artist milik user yang login (UserId).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Artist")]
    public class ArtistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArtistController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>
        /// Profil artist milik user yang login. Bila belum ada, dibuat otomatis
        /// (menggunakan nama user) agar user ber-role Artist langsung bisa berkarya.
        /// </summary>
        private async Task<Artist> GetOrCreateOwnArtistAsync()
        {
            var userId = CurrentUserId;
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
            if (artist != null) return artist;

            var user = await _context.Users.FindAsync(userId);
            artist = new Artist
            {
                UserId = userId,
                Name = user?.Name ?? "Artist",
                Bio = "",
                Genre = "",
                City = user?.City ?? "",
                PhotoUrl = user?.PhotoUrl ?? ""
            };
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        private async Task<Artist?> GetOwnArtistAsync()
        {
            return await _context.Artists.FirstOrDefaultAsync(a => a.UserId == CurrentUserId);
        }

        // ── Profil ────────────────────────────────────────

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var artist = await GetOrCreateOwnArtistAsync();

            var members = await _context.ArtistMembers
                .Where(m => m.ArtistId == artist.ArtistId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

            return Ok(new
            {
                artist.ArtistId,
                artist.Name,
                artist.Bio,
                artist.PhotoUrl,
                artist.CoverUrl,
                artist.Genre,
                artist.City,
                artist.SocialLinks,
                followersCount = await _context.Follows.CountAsync(f => f.ArtistId == artist.ArtistId),
                tracksCount = await _context.AudioTracks.CountAsync(t => t.ArtistId == artist.ArtistId),
                albumsCount = await _context.ArtistAlbums.CountAsync(al => al.ArtistId == artist.ArtistId),
                postsCount = await _context.ArtistPosts.CountAsync(p => p.ArtistId == artist.ArtistId),
                members
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateArtistProfileRequest request)
        {
            var artist = await GetOrCreateOwnArtistAsync();

            artist.Name = string.IsNullOrWhiteSpace(request.Name) ? artist.Name : request.Name.Trim();
            artist.Bio = request.Bio ?? artist.Bio;
            artist.PhotoUrl = request.PhotoUrl ?? artist.PhotoUrl;
            artist.CoverUrl = request.CoverUrl ?? artist.CoverUrl;
            artist.Genre = request.Genre ?? artist.Genre;
            artist.City = request.City ?? artist.City;
            artist.SocialLinks = request.SocialLinks ?? artist.SocialLinks;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profil artist diperbarui", artistId = artist.ArtistId });
        }

        /// <summary>Statistik & gigs mendatang untuk tab Dashboard Artist (§7).</summary>
        [HttpGet("me/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var artist = await GetOrCreateOwnArtistAsync();

            var followersCount = await _context.Follows.CountAsync(f => f.ArtistId == artist.ArtistId);
            var tracksCount = await _context.AudioTracks.CountAsync(t => t.ArtistId == artist.ArtistId);
            var albumsCount = await _context.ArtistAlbums.CountAsync(al => al.ArtistId == artist.ArtistId);
            var postsCount = await _context.ArtistPosts.CountAsync(p => p.ArtistId == artist.ArtistId);

            var upcomingGigs = await _context.EventArtists
                .Where(ea => ea.ArtistId == artist.ArtistId && ea.Event!.StartDate >= DateTime.UtcNow && ea.Event.Status == "Published")
                .Select(ea => new
                {
                    ea.Event!.EventId,
                    ea.Event.Name,
                    ea.Event.PosterUrl,
                    ea.Event.StartDate,
                    ea.Event.Status,
                    venueName = ea.Event.Venue!.Name,
                    venueCity = ea.Event.Venue!.City
                })
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return Ok(new
            {
                artist.ArtistId,
                artist.Name,
                artist.PhotoUrl,
                artist.CoverUrl,
                artist.Genre,
                artist.City,
                followersCount,
                tracksCount,
                albumsCount,
                postsCount,
                upcomingGigs
            });
        }

        // ── Tracks ────────────────────────────────────────

        [HttpPost("tracks")]
        public async Task<IActionResult> CreateTrack([FromBody] TrackRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Judul lagu wajib diisi" });

            var artist = await GetOrCreateOwnArtistAsync();
            var track = new AudioTrack
            {
                ArtistId = artist.ArtistId,
                Title = request.Title.Trim(),
                AudioUrl = request.AudioUrl ?? "",
                CoverUrl = request.CoverUrl ?? "",
                Genre = request.Genre ?? "",
                DurationSeconds = Math.Max(0, request.DurationSeconds ?? 30),
                ReleaseDate = request.ReleaseDate
            };
            _context.AudioTracks.Add(track);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Lagu ditambahkan", track });
        }

        [HttpPut("tracks/{id}")]
        public async Task<IActionResult> UpdateTrack(int id, [FromBody] TrackRequest request)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var track = await _context.AudioTracks.FirstOrDefaultAsync(t => t.TrackId == id && t.ArtistId == artist.ArtistId);
            if (track == null) return NotFound(new { message = "Lagu tidak ditemukan" });

            if (!string.IsNullOrWhiteSpace(request.Title)) track.Title = request.Title.Trim();
            track.AudioUrl = request.AudioUrl ?? track.AudioUrl;
            track.CoverUrl = request.CoverUrl ?? track.CoverUrl;
            track.Genre = request.Genre ?? track.Genre;
            track.DurationSeconds = request.DurationSeconds ?? track.DurationSeconds;
            track.ReleaseDate = request.ReleaseDate ?? track.ReleaseDate;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lagu diperbarui" });
        }

        [HttpDelete("tracks/{id}")]
        public async Task<IActionResult> DeleteTrack(int id)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var track = await _context.AudioTracks.FirstOrDefaultAsync(t => t.TrackId == id && t.ArtistId == artist.ArtistId);
            if (track == null) return NotFound(new { message = "Lagu tidak ditemukan" });

            _context.AudioTracks.Remove(track);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Lagu dihapus" });
        }

        // ── Albums ────────────────────────────────────────

        [HttpPost("albums")]
        public async Task<IActionResult> CreateAlbum([FromBody] AlbumRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Judul album wajib diisi" });

            var artist = await GetOrCreateOwnArtistAsync();
            var album = new ArtistAlbum
            {
                ArtistId = artist.ArtistId,
                Title = request.Title.Trim(),
                CoverUrl = request.CoverUrl ?? "",
                Description = request.Description ?? "",
                ReleaseDate = request.ReleaseDate
            };
            _context.ArtistAlbums.Add(album);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Album ditambahkan", album });
        }

        [HttpPut("albums/{id}")]
        public async Task<IActionResult> UpdateAlbum(int id, [FromBody] AlbumRequest request)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var album = await _context.ArtistAlbums.FirstOrDefaultAsync(al => al.AlbumId == id && al.ArtistId == artist.ArtistId);
            if (album == null) return NotFound(new { message = "Album tidak ditemukan" });

            if (!string.IsNullOrWhiteSpace(request.Title)) album.Title = request.Title.Trim();
            album.CoverUrl = request.CoverUrl ?? album.CoverUrl;
            album.Description = request.Description ?? album.Description;
            album.ReleaseDate = request.ReleaseDate ?? album.ReleaseDate;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Album diperbarui" });
        }

        [HttpDelete("albums/{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var album = await _context.ArtistAlbums.FirstOrDefaultAsync(al => al.AlbumId == id && al.ArtistId == artist.ArtistId);
            if (album == null) return NotFound(new { message = "Album tidak ditemukan" });

            _context.ArtistAlbums.Remove(album);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Album dihapus" });
        }

        // ── Posts ─────────────────────────────────────────

        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost([FromBody] PostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Judul post wajib diisi" });

            var artist = await GetOrCreateOwnArtistAsync();
            var post = new ArtistPost
            {
                ArtistId = artist.ArtistId,
                Title = request.Title.Trim(),
                Content = request.Content ?? "",
                ImageUrl = request.ImageUrl ?? "",
                IsPublished = request.IsPublished ?? true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ArtistPosts.Add(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post dibuat", post });
        }

        [HttpPut("posts/{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] PostRequest request)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var post = await _context.ArtistPosts.FirstOrDefaultAsync(p => p.PostId == id && p.ArtistId == artist.ArtistId);
            if (post == null) return NotFound(new { message = "Post tidak ditemukan" });

            if (!string.IsNullOrWhiteSpace(request.Title)) post.Title = request.Title.Trim();
            post.Content = request.Content ?? post.Content;
            post.ImageUrl = request.ImageUrl ?? post.ImageUrl;
            post.IsPublished = request.IsPublished ?? post.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Post diperbarui" });
        }

        [HttpDelete("posts/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var post = await _context.ArtistPosts.FirstOrDefaultAsync(p => p.PostId == id && p.ArtistId == artist.ArtistId);
            if (post == null) return NotFound(new { message = "Post tidak ditemukan" });

            _context.ArtistPosts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post dihapus" });
        }

        // ── Journey ───────────────────────────────────────

        [HttpPost("journey")]
        public async Task<IActionResult> CreateJourney([FromBody] JourneyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Judul perjalanan wajib diisi" });

            var artist = await GetOrCreateOwnArtistAsync();
            var item = new ArtistJourneyItem
            {
                ArtistId = artist.ArtistId,
                Title = request.Title.Trim(),
                Description = request.Description ?? "",
                ImageUrl = request.ImageUrl ?? "",
                Category = request.Category ?? "Other",
                Date = request.Date
            };
            _context.ArtistJourneyItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Perjalanan ditambahkan", item });
        }

        [HttpPut("journey/{id}")]
        public async Task<IActionResult> UpdateJourney(int id, [FromBody] JourneyRequest request)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var item = await _context.ArtistJourneyItems.FirstOrDefaultAsync(j => j.JourneyId == id && j.ArtistId == artist.ArtistId);
            if (item == null) return NotFound(new { message = "Perjalanan tidak ditemukan" });

            if (!string.IsNullOrWhiteSpace(request.Title)) item.Title = request.Title.Trim();
            item.Description = request.Description ?? item.Description;
            item.ImageUrl = request.ImageUrl ?? item.ImageUrl;
            item.Category = request.Category ?? item.Category;
            item.Date = request.Date ?? item.Date;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Perjalanan diperbarui" });
        }

        [HttpDelete("journey/{id}")]
        public async Task<IActionResult> DeleteJourney(int id)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var item = await _context.ArtistJourneyItems.FirstOrDefaultAsync(j => j.JourneyId == id && j.ArtistId == artist.ArtistId);
            if (item == null) return NotFound(new { message = "Perjalanan tidak ditemukan" });

            _context.ArtistJourneyItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Perjalanan dihapus" });
        }

        // ── Members (anggota band) ────────────────────────

        [HttpPost("members")]
        public async Task<IActionResult> CreateMember([FromBody] MemberRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Nama anggota wajib diisi" });

            var artist = await GetOrCreateOwnArtistAsync();
            var member = new ArtistMember
            {
                ArtistId = artist.ArtistId,
                Name = request.Name.Trim(),
                Role = request.Role ?? "",
                PhotoUrl = request.PhotoUrl ?? "",
                JoinedAt = request.JoinedAt
            };
            _context.ArtistMembers.Add(member);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Anggota ditambahkan", member });
        }

        [HttpDelete("members/{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var artist = await GetOwnArtistAsync();
            if (artist == null) return NotFound(new { message = "Profil artist belum ada" });

            var member = await _context.ArtistMembers.FirstOrDefaultAsync(m => m.MemberId == id && m.ArtistId == artist.ArtistId);
            if (member == null) return NotFound(new { message = "Anggota tidak ditemukan" });

            _context.ArtistMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Anggota dihapus" });
        }
    }

    // ── Request models ───────────────────────────────────

    public class UpdateArtistProfileRequest
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? Genre { get; set; }
        public string? City { get; set; }
        public string? SocialLinks { get; set; }
    }

    public class TrackRequest
    {
        public string? Title { get; set; }
        public string? AudioUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? Genre { get; set; }
        public int? DurationSeconds { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }

    public class AlbumRequest
    {
        public string? Title { get; set; }
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }

    public class PostRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsPublished { get; set; }
    }

    public class JourneyRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public DateTime? Date { get; set; }
    }

    public class MemberRequest
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime? JoinedAt { get; set; }
    }
}