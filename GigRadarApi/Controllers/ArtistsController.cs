using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    /// <summary>
    /// API publik Artist (GIGRADAR_ROLE_SYSTEM.md §31) — dipakai User & EO untuk
    /// discovery: profil, musik, album, post, journey, dan gigs (lineup event).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArtistsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetArtists()
        {
            var artists = await _context.Artists
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.ArtistId,
                    a.Name,
                    a.PhotoUrl,
                    a.CoverUrl,
                    a.Genre,
                    a.City,
                    a.Bio,
                    followersCount = a.Followers.Count(f => f.ArtistId == a.ArtistId),
                    tracksCount = a.Tracks.Count
                })
                .ToListAsync();
            return Ok(artists);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtist(int id)
        {
            var artist = await _context.Artists
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null) return NotFound(new { message = "Artist tidak ditemukan" });

            var members = await _context.ArtistMembers
                .Where(m => m.ArtistId == id)
                .OrderBy(m => m.JoinedAt)
                .Select(m => new { m.MemberId, m.Name, m.Role, m.PhotoUrl, m.JoinedAt })
                .ToListAsync();

            var tracks = await _context.AudioTracks
                .Where(t => t.ArtistId == id)
                .OrderByDescending(t => t.ReleaseDate)
                .Select(t => new { t.TrackId, t.Title, t.AudioUrl, t.CoverUrl, t.Genre, t.DurationSeconds, t.ReleaseDate })
                .ToListAsync();

            var albums = await _context.ArtistAlbums
                .Where(al => al.ArtistId == id)
                .OrderByDescending(al => al.ReleaseDate)
                .Select(al => new { al.AlbumId, al.Title, al.CoverUrl, al.Description, al.ReleaseDate })
                .ToListAsync();

            var posts = await _context.ArtistPosts
                .Where(p => p.ArtistId == id && p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.PostId, p.Title, p.Content, p.ImageUrl, p.CreatedAt, p.UpdatedAt })
                .ToListAsync();

            var journey = await _context.ArtistJourneyItems
                .Where(j => j.ArtistId == id)
                .OrderBy(j => j.Date)
                .Select(j => new { j.JourneyId, j.Title, j.Description, j.ImageUrl, j.Category, j.Date })
                .ToListAsync();

            var followersCount = await _context.Follows.CountAsync(f => f.ArtistId == id);

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
                followersCount,
                members,
                tracks,
                albums,
                posts,
                journey
            });
        }

        [HttpGet("{id}/tracks")]
        public async Task<IActionResult> GetTracks(int id)
        {
            var tracks = await _context.AudioTracks
                .Where(t => t.ArtistId == id)
                .OrderByDescending(t => t.ReleaseDate)
                .Select(t => new { t.TrackId, t.Title, t.AudioUrl, t.CoverUrl, t.Genre, t.DurationSeconds, t.ReleaseDate })
                .ToListAsync();
            return Ok(tracks);
        }

        [HttpGet("{id}/albums")]
        public async Task<IActionResult> GetAlbums(int id)
        {
            var albums = await _context.ArtistAlbums
                .Where(al => al.ArtistId == id)
                .OrderByDescending(al => al.ReleaseDate)
                .Select(al => new { al.AlbumId, al.Title, al.CoverUrl, al.Description, al.ReleaseDate })
                .ToListAsync();
            return Ok(albums);
        }

        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetPosts(int id)
        {
            var posts = await _context.ArtistPosts
                .Where(p => p.ArtistId == id && p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.PostId, p.Title, p.Content, p.ImageUrl, p.CreatedAt, p.UpdatedAt })
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}/journey")]
        public async Task<IActionResult> GetJourney(int id)
        {
            var journey = await _context.ArtistJourneyItems
                .Where(j => j.ArtistId == id)
                .OrderBy(j => j.Date)
                .Select(j => new { j.JourneyId, j.Title, j.Description, j.ImageUrl, j.Category, j.Date })
                .ToListAsync();
            return Ok(journey);
        }

        /// <summary>Gigs — event yang menampilkan artist ini di line-up (§18/§46).</summary>
        [HttpGet("{id}/events")]
        public async Task<IActionResult> GetArtistEvents(int id)
        {
            var events = await _context.EventArtists
                .Where(ea => ea.ArtistId == id)
                .Select(ea => new
                {
                    ea.Event!.EventId,
                    ea.Event.Name,
                    ea.Event.PosterUrl,
                    ea.Event.StartDate,
                    ea.Event.EndDate,
                    ea.Event.Status,
                    venueName = ea.Event.Venue!.Name,
                    venueCity = ea.Event.Venue!.City,
                    ea.Event.MinPrice,
                    ea.Event.MaxPrice
                })
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
            return Ok(events);
        }
    }
}