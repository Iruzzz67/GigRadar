using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GigRadarApi.Services;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var tickets = await _ticketService.GetUserTicketsAsync(userId);
            return Ok(tickets);
        }

        /// <summary>
        /// Daftar peserta event (EO/Admin pemiliknya). Data kontak pembeli sengaja tidak
        /// dikirim — halaman peserta hanya butuh nama, tipe tiket, dan status kehadiran.
        /// </summary>
        [HttpGet("event/{eventId}/attendees")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> GetEventAttendees(int eventId)
        {
            var (attendees, error, forbidden) = await _ticketService.GetEventAttendeesAsync(
                eventId, GetUserId(), GetRole());

            if (forbidden)
                return StatusCode(403, new { message = error });
            if (attendees == null)
                return NotFound(new { message = error });

            return Ok(new
            {
                eventId,
                total = attendees.Count,
                checkedIn = attendees.Count(t => t.Status == "Used"),
                active = attendees.Count(t => t.Status == "Active"),
                attendees = attendees.Select(t => new
                {
                    t.TicketId,
                    t.TicketType,
                    t.BuyerName,
                    t.Price,
                    t.Status,
                    t.PurchasedAt
                })
            });
        }

        [HttpGet("event/{eventId}/types")]
        public async Task<IActionResult> GetEventTicketTypes(int eventId)
        {
            var types = await _ticketService.GetEventTicketTypesAsync(eventId);
            return Ok(types);
        }

        // ── Kelola tipe tiket (halaman admin/EO) ─────────

        [HttpPost("event/{eventId}/types")]
        [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
        public async Task<IActionResult> AddTicketType(int eventId, [FromBody] EventTicketType type)
        {
            var (created, error) = await _ticketService.AddTicketTypeAsync(
                eventId, type, GetUserId(), GetRole());

            if (created == null)
                return BadRequest(new { message = error ?? "Gagal menambah tipe tiket" });

            return StatusCode(201, new { message = "Tipe tiket ditambahkan", ticketType = created });
        }

        [HttpPut("types/{typeId}")]
        [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
        public async Task<IActionResult> UpdateTicketType(int typeId, [FromBody] EventTicketType type)
        {
            var (updated, error) = await _ticketService.UpdateTicketTypeAsync(
                typeId, type, GetUserId(), GetRole());

            if (updated == null)
                return BadRequest(new { message = error ?? "Gagal memperbarui tipe tiket" });

            return Ok(new { message = "Tipe tiket diperbarui", ticketType = updated });
        }

        [HttpDelete("types/{typeId}")]
        [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
        public async Task<IActionResult> DeleteTicketType(int typeId)
        {
            var (deleted, error) = await _ticketService.DeleteTicketTypeAsync(
                typeId, GetUserId(), GetRole());

            if (!deleted)
                return BadRequest(new { message = error ?? "Gagal menghapus tipe tiket" });

            return Ok(new { message = "Tipe tiket dihapus", id = typeId });
        }

        private int GetUserId() => int.Parse(User.FindFirst("UserId")!.Value);
        private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound(new { message = "Tiket tidak ditemukan" });
            return Ok(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);

            var (ticket, error) = await _ticketService.PurchaseTicketAsync(
                userId,
                request.EventId,
                request.TicketTypeId,
                request.FullName,
                request.Phone,
                request.Email,
                request.DateOfBirth);

            if (ticket == null)
                return BadRequest(new { message = error ?? "Gagal membeli tiket" });

            return Ok(new { message = "Tiket berhasil dibeli", ticket });
        }

        /// <summary>
        /// Check-in tiket via QR (Bab 14). Tiket harus milik event yang dikelola EO/Admin
        /// yang men-scan — bukan sekadar "tiket valid mana pun" (§24). Respons menyertakan
        /// ringkasan kehadiran supaya halaman check-in bisa menampilkan progres.
        /// </summary>
        [HttpPost("validate")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketRequest request)
        {
            if (request == null)
                return BadRequest(new { valid = false, message = "Body check-in wajib dikirim" });

            var (ticket, error, forbidden) = await _ticketService.CheckInTicketAsync(
                request.QRCode, request.EventId, GetUserId(), GetRole());

            if (forbidden)
                return StatusCode(403, new { valid = false, message = error });

            var (checkedIn, total) = await _ticketService.GetCheckInCountsAsync(request.EventId);

            if (ticket == null)
                return Ok(new { valid = false, message = error, checkedIn, total });

            return Ok(new
            {
                valid = true,
                message = $"Check-in berhasil: {ticket.BuyerName} ({ticket.TicketType})",
                checkedIn,
                total,
                ticket = new
                {
                    ticket.TicketId,
                    ticket.TicketType,
                    ticket.BuyerName,
                    ticket.Status,
                    ticket.PurchasedAt
                }
            });
        }
    }

    public class PurchaseTicketRequest
    {
        public int EventId { get; set; }
        public int TicketTypeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
    }

    public class ValidateTicketRequest
    {
        public string QRCode { get; set; } = string.Empty;

        /// <summary>Event yang sedang di-check-in — dipakai untuk memvalidasi kepemilikan oleh EO (§24).</summary>
        public int EventId { get; set; }
    }
}