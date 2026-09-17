using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    /// <summary>
    /// Interaksi sosial pada post artist (GIGRADAR_APP_SPEC.md Bab 32, 48):
    /// like (toggle idempoten), daftar komentar, dan tulis komentar.
    /// Semua role authenticated boleh berinteraksi; konten post milik artist.
    /// </summary>
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>Like / unlike post (toggle idempoten — Bab 48).</summary>
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var post = await _context.ArtistPosts.FindAsync(postId);
            if (post == null) return NotFound(new { message = "Post tidak ditemukan" });

            var userId = CurrentUserId;
            var existing = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existing != null)
            {
                _context.PostLikes.Remove(existing);
                await _context.SaveChangesAsync();
                var countAfter = await _context.PostLikes.CountAsync(l => l.PostId == postId);
                return Ok(new { message = "Like dihapus", liked = false, likesCount = countAfter });
            }

            _context.PostLikes.Add(new PostLike { PostId = postId, UserId = userId });
            await _context.SaveChangesAsync();
            var count = await _context.PostLikes.CountAsync(l => l.PostId == postId);
            return Ok(new { message = "Post disukai", liked = true, likesCount = count });
        }

        /// <summary>Daftar komentar post (terlama dulu — alur percakapan natural).</summary>
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var post = await _context.ArtistPosts.FindAsync(postId);
            if (post == null) return NotFound(new { message = "Post tidak ditemukan" });

            var comments = await _context.PostComments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.CommentId,
                    c.PostId,
                    c.UserId,
                    userName = c.User!.Name,
                    userPhotoUrl = c.User.PhotoUrl,
                    c.Content,
                    c.CreatedAt
                })
                .ToListAsync();
            return Ok(comments);
        }

        /// <summary>Tulis komentar pada post (konten wajib ada, 1–1000 karakter).</summary>
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] AddCommentRequest request)
        {
            var content = request.Content?.Trim() ?? "";
            if (content.Length == 0)
                return BadRequest(new { message = "Komentar tidak boleh kosong" });
            if (content.Length > 1000)
                return BadRequest(new { message = "Komentar maksimal 1000 karakter" });

            var post = await _context.ArtistPosts.FindAsync(postId);
            if (post == null) return NotFound(new { message = "Post tidak ditemukan" });
            if (!post.IsPublished)
                return BadRequest(new { message = "Post tidak tersedia" });

            var comment = new PostComment
            {
                PostId = postId,
                UserId = CurrentUserId,
                Content = content
            };
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(CurrentUserId);
            return Ok(new
            {
                message = "Komentar terkirim",
                comment = new
                {
                    comment.CommentId,
                    comment.PostId,
                    comment.UserId,
                    userName = user?.Name ?? "",
                    userPhotoUrl = user?.PhotoUrl ?? "",
                    comment.Content,
                    comment.CreatedAt
                }
            });
        }
    }

    public class AddCommentRequest
    {
        public string? Content { get; set; }
    }
}
