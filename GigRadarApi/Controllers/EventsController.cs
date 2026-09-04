using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GigRadarApi.Services;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt == null) return NotFound(new { message = "Event tidak ditemukan" });
            return Ok(evt);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyEvents([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 50)
        {
            var events = await _eventService.GetNearbyEventsAsync(lat, lng, radius);
            return Ok(events);
        }

        [HttpGet("tonight")]
        public async Task<IActionResult> GetTonightEvents()
        {
            var events = await _eventService.GetTonightEventsAsync();
            return Ok(events);
        }

        [HttpGet("weekend")]
        public async Task<IActionResult> GetWeekendEvents()
        {
            var events = await _eventService.GetWeekendEventsAsync();
            return Ok(events);
        }

        [HttpGet("managed")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> GetManagedEvents()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";
            var events = await _eventService.GetManagedEventsAsync(userId, role);
            return Ok(events);
        }

        [HttpGet("managed/summary")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> GetManagedSummary()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";
            var summary = await _eventService.GetManagedSummaryAsync(userId, role);
            return Ok(summary);
        }

        [HttpGet("recommended")]
        [Authorize]
        public async Task<IActionResult> GetRecommendedEvents()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var events = await _eventService.GetRecommendedEventsAsync(userId);
            return Ok(events);
        }

        [HttpPost]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> CreateEvent([FromBody] Event evt)
        {
            var validation = ValidateEvent(evt);
            if (validation != null) return validation;

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            evt.CreatedBy = userId;

            var created = await _eventService.CreateEventAsync(evt);
            if (created == null)
                return BadRequest(new { message = "Gagal membuat event" });

            return StatusCode(201, new { message = "Event created", id = created.EventId, event_ = created });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event evt)
        {
            var validation = ValidateEvent(evt);
            if (validation != null) return validation;

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";

            var (success, error, forbidden) = await _eventService.UpdateEventAsync(id, evt, userId, role);
            if (forbidden)
                return StatusCode(403, new { message = error });
            if (!success)
                return NotFound(new { message = error });

            return Ok(new { message = "Event updated", id });
        }

        /// <summary>Ubah status event: Published / Draft / SoldOut / Completed.</summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> ChangeEventStatus(int id, [FromBody] ChangeEventStatusRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";

            var (evt, error, forbidden) = await _eventService.ChangeEventStatusAsync(
                id, userId, role, request?.Status ?? string.Empty);

            if (forbidden)
                return StatusCode(403, new { message = error });
            if (evt == null)
                return BadRequest(new { message = error });

            return Ok(new { message = "Status event diperbarui", id = evt.EventId, status = evt.Status });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";

            var (deleted, error, forbidden) = await _eventService.DeleteEventAsync(id, userId, role);
            if (forbidden)
                return StatusCode(403, new { message = error });
            if (!deleted)
                return NotFound(new { message = error });

            return Ok(new { message = "Event deleted", id });
        }

        public class ChangeEventStatusRequest
        {
            public string? Status { get; set; }
        }

        private IActionResult? ValidateEvent(Event evt)
        {
            if (evt == null)
                return BadRequest(new { message = "Body event wajib dikirim" });

            if (string.IsNullOrWhiteSpace(evt.Name))
                return BadRequest(new { message = "Nama event wajib diisi" });

            if (evt.EndDate < evt.StartDate)
                return BadRequest(new { message = "EndDate tidak boleh sebelum StartDate" });

            if (evt.MinPrice < 0 || evt.MaxPrice < 0)
                return BadRequest(new { message = "Harga tidak boleh negatif" });

            if (evt.MinPrice > evt.MaxPrice)
                return BadRequest(new { message = "MinPrice tidak boleh lebih besar dari MaxPrice" });

            if (evt.Capacity < 0)
                return BadRequest(new { message = "Kapasitas tidak boleh negatif" });

            return null;
        }
    }
}
