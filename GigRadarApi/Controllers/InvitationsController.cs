using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;

namespace GigRadarApi.Controllers
{
    /// <summary>
    /// Undangan artist ↔ EO (GIGRADAR_APP_SPEC.md Bab 25–26, 48):
    /// - EO mengundang artist ke event (event milik sendiri).
    /// - Artist accept/reject undangan yang ditujukan kepadanya.
    /// - EO dapat membatalkan undangan yang masih PENDING.
    /// Accept menambahkan artist ke line-up (EventArtist) bila belum ada.
    /// </summary>
    [Route("api")]
    [ApiController]
    [Authorize]
    public class InvitationsController(AppDbContext context, NotificationService notifications) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        // ── EO: buat & kelola undangan ────────────────────

        /// <summary>EO mengundang artist ke event miliknya (POST /api/events/{id}/invitations).</summary>
        [HttpPost("events/{eventId}/invitations")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> InviteArtist(int eventId, [FromBody] CreateInvitationRequest request)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event tidak ditemukan" });

            // Ownership: EO hanya boleh mengundang ke event miliknya (Bab 79 #5).
            if (!User.IsInRole("Admin") && ev.CreatedBy != CurrentUserId)
                return Forbid();

            var artist = await _context.Artists.FindAsync(request.ArtistId);
            if (artist == null) return NotFound(new { message = "Artist tidak ditemukan" });
            // Artist yang sudah ada di line-up tidak perlu diundang lagi.
            var inLineup = await _context.EventArtists
                .AnyAsync(ea => ea.EventId == eventId && ea.ArtistId == request.ArtistId);
            if (inLineup)
                return BadRequest(new { message = "Artist sudah ada di line-up event ini" });

            // Satu undangan aktif (PENDING) per pasangan event-artist (Bab 26).
            var hasPending = await _context.EventInvitations
                .AnyAsync(i => i.EventId == eventId && i.ArtistId == request.ArtistId && i.Status == "PENDING");
            if (hasPending)
                return BadRequest(new { message = "Undangan untuk artist ini masih menunggu jawaban" });

            // Undangan ACCEPTED/REJECTED lama boleh dibuat ulang (line-up sudah dicek di atas).
            var invitation = new EventInvitation
            {
                EventId = eventId,
                ArtistId = artist.ArtistId,
                CreatedByUserId = CurrentUserId,
                Message = request.Message?.Trim() ?? "",
                Status = "PENDING"
            };
            _context.EventInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Undangan terkirim ke \"{artist.Name}\"", invitation.InvitationId });
        }

        /// <summary>Daftar undangan untuk satu event (milik EO yang login).</summary>
        [HttpGet("events/{eventId}/invitations")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> GetEventInvitations(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event tidak ditemukan" });

            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin" && ev.CreatedBy != CurrentUserId)
                return Forbid();

            var invitations = await _context.EventInvitations
                .Where(i => i.EventId == eventId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.InvitationId,
                    i.EventId,
                    i.ArtistId,
                    artistName = i.Artist!.Name,
                    artistGenre = i.Artist.Genre,
                    i.Message,
                    i.Status,
                    i.CreatedAt,
                    i.RespondedAt
                })
                .ToListAsync();
            return Ok(invitations);
        }

        /// <summary>Daftar undangan untuk artist yang login (diterima & terkirim).</summary>
        [HttpGet("invitations/me")]
        public async Task<IActionResult> GetMyInvitations()
        {
            var userId = CurrentUserId;
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);

            var sent = await _context.EventInvitations
                .Where(i => i.CreatedByUserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.InvitationId,
                    i.EventId,
                    eventName = i.Event!.Name,
                    i.ArtistId,
                    artistName = i.Artist!.Name,
                    i.Message,
                    i.Status,
                    i.CreatedAt,
                    i.RespondedAt,
                    direction = "sent"
                })
                .ToListAsync();

            if (artist == null) return Ok(new { received = Array.Empty<object>(), sent });

            var received = await _context.EventInvitations
                .Where(i => i.ArtistId == artist.ArtistId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.InvitationId,
                    i.EventId,
                    eventName = i.Event!.Name,
                    eventDate = i.Event.StartDate,
                    venueName = i.Event.Venue!.Name,
                    i.ArtistId,
                    artistName = i.Artist.Name,
                    i.Message,
                    i.Status,
                    i.CreatedAt,
                    i.RespondedAt,
                    direction = "received"
                })
                .ToListAsync();

            return Ok(new { received, sent });
        }

        /// <summary>Artist menerima undangan — masuk line-up event (Bab 25).</summary>
        [HttpPost("invitations/{invitationId}/accept")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> AcceptInvitation(int invitationId)
        {
            var (success, message) = await RespondAsync(invitationId, "ACCEPTED");
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>Artist menolak undangan.</summary>
        [HttpPost("invitations/{invitationId}/reject")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> RejectInvitation(int invitationId)
        {
            var (success, message) = await RespondAsync(invitationId, "REJECTED");
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        /// <summary>EO membatalkan undangan yang masih PENDING (Bab 26 — CANCELLED).</summary>
        [HttpPost("invitations/{invitationId}/cancel")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> CancelInvitation(int invitationId)
        {
            var invitation = await _context.EventInvitations
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.InvitationId == invitationId);
            if (invitation == null)
                return NotFound(new { message = "Undangan tidak ditemukan" });

            if (!User.IsInRole("Admin") && invitation.Event!.CreatedBy != CurrentUserId)
                return Forbid();

            if (invitation.Status != "PENDING")
                return BadRequest(new { message = "Hanya undangan yang masih menunggu yang bisa dibatalkan" });

            invitation.Status = "CANCELLED";
            invitation.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Undangan dibatalkan" });
        }

        /// <summary>Inti respond: validasi penerima + status, lalu terapkan (accept → line-up).</summary>
        private async Task<(bool Success, string Message)> RespondAsync(int invitationId, string decision)
        {
            var userId = CurrentUserId;
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
            if (artist == null)
                return (false, "Profil artist belum ada");

            var invitation = await _context.EventInvitations
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.InvitationId == invitationId);
            if (invitation == null)
                return (false, "Undangan tidak ditemukan");

            // Ownership: hanya artist penerima undangan yang bisa merespons.
            if (invitation.ArtistId != artist.ArtistId)
                return (false, "Undangan ini bukan untuk artist kamu");

            if (invitation.Status != "PENDING")
                return (false, "Undangan ini sudah diputuskan");

            invitation.Status = decision;
            invitation.RespondedAt = DateTime.UtcNow;

            // Notifikasi ke EO pembuat undangan (Bab 36 — ARTIST_INVITATION).
            await notifications.NotifyAsync(invitation.CreatedByUserId, "ARTIST_INVITATION",
                decision == "ACCEPTED" ? "Undangan diterima" : "Undangan ditolak",
                $"\"{artist.Name}\" {(decision == "ACCEPTED" ? "menerima" : "menolak")} undangan untuk \"{invitation.Event!.Name}\".",
                $"events/{invitation.EventId}");

            if (decision == "ACCEPTED")
            {
                var alreadyInLineup = await _context.EventArtists
                    .AnyAsync(ea => ea.EventId == invitation.EventId && ea.ArtistId == artist.ArtistId);
                if (!alreadyInLineup)
                {
                    var maxOrder = await _context.EventArtists
                        .Where(ea => ea.EventId == invitation.EventId)
                        .AnyAsync()
                        ? await _context.EventArtists.Where(ea => ea.EventId == invitation.EventId)
                            .MaxAsync(ea => ea.Order)
                        : 0;
                    _context.EventArtists.Add(new EventArtist
                    {
                        EventId = invitation.EventId,
                        ArtistId = artist.ArtistId,
                        Order = maxOrder + 1
                    });
                }
            }

            await _context.SaveChangesAsync();
            var msg = decision == "ACCEPTED"
                ? $"Undangan diterima — kamu masuk line-up \"{invitation.Event!.Name}\""
                : "Undangan ditolak";
            return (true, msg);
        }
    }

    public class CreateInvitationRequest
    {
        public int? ArtistId { get; set; }
        public string? Message { get; set; }
    }
}
