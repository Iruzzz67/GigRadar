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
    public class CommunityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommunityController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        [HttpGet("artists/{artistId}/posts")]
        public async Task<IActionResult> GetCommunityPosts(int artistId)
        {
            var posts = await _context.CommunityPosts
                .Include(p => p.User)
                .Where(p => p.ArtistId == artistId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.CommunityPostId,
                    p.ArtistId,
                    p.UserId,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedAt,
                    UserName = p.User != null ? p.User.Name : "Unknown",
                    UserPhotoUrl = p.User != null ? p.User.PhotoUrl : ""
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost("artists/{artistId}/posts")]
        public async Task<IActionResult> CreatePost(int artistId, [FromBody] CommunityPost request)
        {
            var artistExists = await _context.Artists.AnyAsync(a => a.ArtistId == artistId);
            if (!artistExists) return NotFound("Artist not found");

            var post = new CommunityPost
            {
                ArtistId = artistId,
                UserId = CurrentUserId,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommunityPosts), new { artistId = artistId }, post);
        }
    }
}
