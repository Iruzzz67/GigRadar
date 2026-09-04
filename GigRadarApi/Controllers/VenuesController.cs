using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VenuesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Daftar semua venue — dipakai form pembuatan event (pemilihan tempat).</summary>
        [HttpGet]
        public async Task<IActionResult> GetVenues()
        {
            var venues = await _context.Venues.OrderBy(v => v.Name).ToListAsync();
            return Ok(venues);
        }

        /// <summary>
        /// Tambah venue baru. Dipakai EO/Admin saat membuat event di tempat yang
        /// belum terdaftar di aplikasi.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> CreateVenue([FromBody] Venue venue)
        {
            if (venue == null || string.IsNullOrWhiteSpace(venue.Name))
                return BadRequest(new { message = "Nama venue wajib diisi" });

            venue.VenueId = 0;
            venue.Name = venue.Name.Trim();
            venue.City = venue.City?.Trim() ?? string.Empty;
            venue.Address = venue.Address?.Trim() ?? string.Empty;
            venue.PhotoUrl = venue.PhotoUrl?.Trim() ?? string.Empty;
            if (venue.Capacity < 0) venue.Capacity = 0;

            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Venue berhasil ditambahkan", id = venue.VenueId, venue });
        }
    }
}
