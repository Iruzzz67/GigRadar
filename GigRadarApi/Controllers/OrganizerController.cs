using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;

namespace GigRadarApi.Controllers
{
    /// <summary>
    /// API manajemen Organizer/EO (GIGRADAR_APP_SPEC.md Bab 12–15) — khusus role EO.
    /// Profil, portofolio, dan pipeline verifikasi EO. Ownership divalidasi service.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "EO")]
    public class OrganizerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OrganizerVerificationService _verificationService;

        public OrganizerController(AppDbContext context, OrganizerVerificationService verificationService)
        {
            _context = context;
            _verificationService = verificationService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        // ── Profil Organizer (Bab 12) ─────────────────────

        /// <summary>Profil organizer milik user yang login (dibuat otomatis bila belum ada).</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var organizer = await _verificationService.GetOrCreateOwnOrganizerAsync(CurrentUserId);
            var portfolio = await _context.OrganizerPortfolioItems
                .Where(p => p.OrganizerId == organizer.OrganizerId)
                .OrderByDescending(p => p.EventDate)
                .ToListAsync();
            return Ok(new { organizer, portfolio });
        }

        /// <summary>Perbarui profil organizer milik user yang login.</summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateOrganizerProfileRequest request)
        {
            var (success, message) = await _verificationService.UpdateProfileAsync(
                CurrentUserId, request.OrganizationName, request.Description, request.Location,
                request.Contact, request.WebsiteUrl, request.SocialLinks, request.FoundedYear);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // ── Portofolio (Bab 14) ───────────────────────────

        /// <summary>Tambah item portofolio (bukti pengalaman menggelar event).</summary>
        [HttpPost("portfolio")]
        public async Task<IActionResult> AddPortfolioItem([FromBody] PortfolioItemRequest request)
        {
            var (success, message) = await _verificationService.AddPortfolioItemAsync(CurrentUserId, request);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Hapus item portofolio milik sendiri (ownership divalidasi service).</summary>
        [HttpDelete("portfolio/{portfolioItemId}")]
        public async Task<IActionResult> DeletePortfolioItem(int portfolioItemId)
        {
            var (success, message) = await _verificationService.DeletePortfolioItemAsync(CurrentUserId, portfolioItemId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // ── Verifikasi (Bab 13, 15) ───────────────────────

        /// <summary>
        /// Ajukan verifikasi EO. Profil wajib lengkap + minimal satu portofolio;
        /// hanya satu pengajuan PENDING; keputusan diambil Admin (Bab 15).
        /// </summary>
        [HttpPost("verification")]
        public async Task<IActionResult> SubmitVerification([FromBody] OrganizerVerificationRequest request)
        {
            var (success, message, _) = await _verificationService.SubmitVerificationAsync(CurrentUserId, request);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Status verifikasi EO milik user yang login + pengajuan terakhir.</summary>
        [HttpGet("verification/status")]
        public async Task<IActionResult> MyVerificationStatus()
        {
            var status = await _verificationService.GetMyStatusAsync(CurrentUserId);
            if (status == null) return NotFound(new { message = "Profil organizer belum ada" });
            return Ok(status);
        }
    }

    public class UpdateOrganizerProfileRequest
    {
        public string? OrganizationName { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? SocialLinks { get; set; }
        public int? FoundedYear { get; set; }
    }
}
