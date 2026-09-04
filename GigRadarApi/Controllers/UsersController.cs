using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Konsol Admin (§23/§26 AdminShell): daftar seluruh user.
        /// Hanya role Admin.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderBy(u => u.UserId)
                .Select(u => new { u.UserId, u.Name, u.Email, u.Role, u.City, u.PhotoUrl, u.CreatedAt })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users
                .Include(u => u.Preferences).ThenInclude(p => p.Genre)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();
            return Ok(new { user.UserId, user.Name, user.Email, user.Role, user.City, user.PhotoUrl, preferences = user.Preferences });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name = request.Name ?? user.Name;
            user.City = request.City ?? user.City;
            user.PhotoUrl = request.PhotoUrl ?? user.PhotoUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated", user });
        }

        [HttpPost("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] List<int> genreIds)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var existing = await _context.UserPreferences.Where(p => p.UserId == userId).ToListAsync();
            _context.UserPreferences.RemoveRange(existing);

            foreach (var genreId in genreIds)
            {
                _context.UserPreferences.Add(new UserPreference { UserId = userId, GenreId = genreId, Weight = 1.0 });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Preferences updated" });
        }

        [HttpPost("favorites/{eventId}")]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var existing = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Removed from favorites", saved = false });
            }

            _context.Favorites.Add(new Favorite { UserId = userId, EventId = eventId });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Added to favorites", saved = true });
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var favorites = await _context.Favorites
                .Include(f => f.Event).ThenInclude(e => e!.Venue)
                .Where(f => f.UserId == userId)
                .ToListAsync();
            return Ok(favorites);
        }

        // ── Follow artist (§30) ───────────────────────────

        /// <summary>Artist yang diikuti user ini.</summary>
        [HttpGet("follows")]
        public async Task<IActionResult> GetFollows()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var follows = await _context.Follows
                .Where(f => f.UserId == userId && f.ArtistId != null)
                .Select(f => new
                {
                    f.FollowId,
                    artistId = f.ArtistId!.Value,
                    artistName = f.Artist!.Name,
                    artistPhotoUrl = f.Artist!.PhotoUrl,
                    artistGenre = f.Artist!.Genre,
                    f.CreatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            return Ok(follows);
        }

        /// <summary>Ikuti artist (idempoten — follow dua kali tidak menduplikasi).</summary>
        [HttpPost("follows/{artistId}")]
        public async Task<IActionResult> FollowArtist(int artistId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null) return NotFound(new { message = "Artist tidak ditemukan" });

            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);
            if (existing != null)
                return Ok(new { message = "Sudah mengikuti artist ini", followed = true });

            _context.Follows.Add(new Follow { UserId = userId, ArtistId = artistId });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sekarang mengikuti artist ini", followed = true });
        }

        /// <summary>Berhenti mengikuti artist.</summary>
        [HttpDelete("follows/{artistId}")]
        public async Task<IActionResult> UnfollowArtist(int artistId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);
            if (follow == null)
                return Ok(new { message = "Tidak mengikuti artist ini", followed = false });

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Berhenti mengikuti artist", followed = false });
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
