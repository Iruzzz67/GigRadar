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
    public class FeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        [HttpGet("recommended-artists")]
        public async Task<ActionResult<IEnumerable<Artist>>> GetRecommendedArtists()
        {
            int userId = CurrentUserId;

            // Get user preferred genres
            var preferredGenres = await _context.UserPreferences
                .Include(up => up.Genre)
                .Where(up => up.UserId == userId && up.Genre != null)
                .Select(up => up.Genre.Name.ToLower())
                .ToListAsync();

            var query = _context.Artists.AsQueryable();
            
            // Fetch all artists to memory if small dataset or evaluate locally for simpler matching
            var allArtists = await query.ToListAsync();
            
            var recommendedArtists = new List<Artist>();
            
            if (preferredGenres.Any())
            {
                recommendedArtists = allArtists
                    .Where(a => preferredGenres.Contains(a.Genre.ToLower()))
                    .OrderBy(a => Guid.NewGuid()) // Randomize for fresh recommendations
                    .Take(10)
                    .ToList();
            }
            
            // If no recommendations from genres, just return popular/random artists
            if (!recommendedArtists.Any())
            {
                recommendedArtists = allArtists
                    .OrderBy(a => Guid.NewGuid())
                    .Take(10)
                    .ToList();
            }

            return Ok(recommendedArtists);
        }
    }
}
