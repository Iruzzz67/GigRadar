using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly ArtistVerificationService _artistVerificationService;
        private readonly OrganizerVerificationService _organizerVerificationService;
        private readonly EventService _eventService;

        public UsersController(AppDbContext context, AuthService authService,
            ArtistVerificationService artistVerificationService,
            OrganizerVerificationService organizerVerificationService,
            EventService eventService)
        {
            _context = context;
            _authService = authService;
            _artistVerificationService = artistVerificationService;
            _organizerVerificationService = organizerVerificationService;
            _eventService = eventService;
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
                .Select(u => new { u.UserId, u.Name, u.Email, u.Role, u.RoleStatus, u.City, u.PhotoUrl, u.CreatedAt })
                .ToListAsync();
            return Ok(users);
        }

        /// <summary>Data tiga komunitas untuk panel analisis Admin.</summary>
        [HttpGet("communities")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCommunities()
        {
            var users = await _context.Users
                .Where(u => u.Role == "User")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.City,
                    u.PhotoUrl,
                    u.CreatedAt
                })
                .ToListAsync();

            var artists = await _context.Artists
                .OrderBy(a => a.Name)
                .Select(a => new
                {
                    a.ArtistId,
                    a.Name,
                    a.Genre,
                    a.City,
                    a.PhotoUrl,
                    a.IsVerified,
                    AccountEmail = a.User != null ? a.User.Email : string.Empty
                })
                .ToListAsync();

            var eos = await _context.Organizers
                .Include(o => o.User)
                .OrderBy(o => o.OrganizationName)
                .Select(o => new
                {
                    o.OrganizerId,
                    Name = o.OrganizationName,
                    o.Location,
                    o.LogoUrl,
                    o.IsVerified,
                    AccountEmail = o.User != null ? o.User.Email : string.Empty
                })
                .ToListAsync();

            return Ok(new { users, artists, eos });
        }

        // ── Admin: CRUD akun (§4.4, §19, §23) ────────────

        /// <summary>Admin membuat akun baru (User/Artist/EO/Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest? request)
        {
            if (request == null)
                return BadRequest(new { message = "Body akun wajib dikirim" });

            var (success, message, user) = await _authService.CreateAccountAsync(
                request.Name ?? "", request.Email ?? "", request.Password ?? "",
                request.Role ?? "", request.City ?? "");

            if (!success) return BadRequest(new { message });

            return StatusCode(201, new
            {
                message,
                user = new { user!.UserId, user.Name, user.Email, user.Role, user.RoleStatus, user.City }
            });
        }

        /// <summary>Admin memperbarui akun. Field null/kosong = nilai lama dipertahankan.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountRequest? request)
        {
            if (request == null)
                return BadRequest(new { message = "Body akun wajib dikirim" });

            var (success, message) = await _authService.UpdateAccountAsync(
                id, request.Name, request.Email, request.Password, request.Role, request.City);

            if (!success) return BadRequest(new { message });
            return Ok(new { message, id });
        }

        /// <summary>Admin menonaktifkan akun — akun & datanya tetap ada, hanya login yang ditolak.</summary>
        [HttpPost("{id}/suspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SuspendAccount(int id)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _authService.SetAccountSuspendedAsync(id, suspended: true, adminId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, id, suspended = true });
        }

        /// <summary>Admin mengaktifkan kembali akun yang dinonaktifkan.</summary>
        [HttpPost("{id}/unsuspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnsuspendAccount(int id)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _authService.SetAccountSuspendedAsync(id, suspended: false, adminId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, id, suspended = false });
        }

        /// <summary>
        /// Admin menghapus akun. Operasi destruktif (§23) — UI wajib menampilkan dialog
        /// konfirmasi sebelum memanggil endpoint ini.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _authService.DeleteAccountAsync(id, adminId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, id });
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

        // ── Kelola permohonan role (§25) — khusus Admin ──

        /// <summary>Daftar permohonan role (default: yang masih Pending).</summary>
        [HttpGet("role-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleRequests([FromQuery] string? status = "Pending")
        {
            var query = _context.RoleRequests.Include(r => r.User).AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            var requests = await query
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    r.RequestId,
                    r.UserId,
                    userName = r.User!.Name,
                    userEmail = r.User!.Email,
                    r.RequestedRole,
                    r.Status,
                    r.CreatedAt
                })
                .ToListAsync();
            return Ok(requests);
        }

        /// <summary>Admin menyetujui permohonan — role user dinaikkan ke role yang dimohonkan.</summary>
        [HttpPost("role-requests/{requestId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRoleRequest(int requestId)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _authService.ApproveRoleRequestAsync(requestId, adminId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Admin menolak permohonan — user tetap berrole User.</summary>
        [HttpPost("role-requests/{requestId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRoleRequest(int requestId)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _authService.RejectRoleRequestAsync(requestId, adminId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // ── Verification Center: verifikasi artist (Bab 22) — khusus Admin ──

        /// <summary>Daftar pengajuan verifikasi artist (query ?status=PENDING|VERIFIED|REJECTED).</summary>
        [HttpGet("artist-verifications")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetArtistVerifications([FromQuery] string? status = "PENDING")
        {
            var items = await _artistVerificationService.GetListAsync(status);
            return Ok(items);
        }

        /// <summary>Admin menyetujui verifikasi — artist mendapat badge verified (Bab 8).</summary>
        [HttpPost("artist-verifications/{verificationId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveArtistVerification(int verificationId)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _artistVerificationService.ReviewAsync(verificationId, adminId, approve: true, reason: null);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Admin menolak verifikasi — alasan wajib agar artist tahu apa yang perlu diperbaiki.</summary>
        [HttpPost("artist-verifications/{verificationId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectArtistVerification(
            int verificationId, [FromBody] RejectVerificationRequest? request)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Alasan penolakan wajib diisi" });

            var (success, message) = await _artistVerificationService.ReviewAsync(
                verificationId, adminId, approve: false, reason: request.Reason);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // ── Verification Center: verifikasi EO (Bab 15, 22) — khusus Admin ──

        /// <summary>Daftar pengajuan verifikasi EO (query ?status=PENDING|VERIFIED|REJECTED).</summary>
        [HttpGet("organizer-verifications")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrganizerVerifications([FromQuery] string? status = "PENDING")
        {
            var items = await _organizerVerificationService.GetListAsync(status);
            return Ok(items);
        }

        /// <summary>Admin menyetujui verifikasi EO — organizer mendapat badge verified (Bab 15).</summary>
        [HttpPost("organizer-verifications/{verificationId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveOrganizerVerification(int verificationId)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            var (success, message) = await _organizerVerificationService.ReviewAsync(
                verificationId, adminId, approve: true, reason: null);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Admin menolak verifikasi EO — alasan wajib agar EO tahu apa yang perlu diperbaiki.</summary>
        [HttpPost("organizer-verifications/{verificationId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectOrganizerVerification(
            int verificationId, [FromBody] RejectVerificationRequest? request)
        {
            var adminId = int.Parse(User.FindFirst("UserId")!.Value);
            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Alasan penolakan wajib diisi" });

            var (success, message) = await _organizerVerificationService.ReviewAsync(
                verificationId, adminId, approve: false, reason: request.Reason);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
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

        /// <summary>
        /// Toggle simpan/lepas event dari daftar tersimpan user. Logikanya ada di
        /// EventService karena sekaligus menjaga Event.SavesCount tetap sinkron (§11).
        /// </summary>
        [HttpPost("favorites/{eventId}")]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var (saved, error) = await _eventService.ToggleSaveAsync(eventId, userId);

            if (error != null)
                return NotFound(new { message = error, saved = false });

            return Ok(new
            {
                message = saved ? "Added to favorites" : "Removed from favorites",
                saved
            });
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

    public class CreateAccountRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? City { get; set; }
    }

    public class UpdateAccountRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        /// <summary>Kosongkan bila password tidak ingin diubah.</summary>
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? City { get; set; }
    }

    public class RejectVerificationRequest
    {
        public string? Reason { get; set; }
    }
}
