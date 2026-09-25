using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GigRadarApi.Services;

namespace GigRadarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly GenreService _genreService;

        public GenresController(GenreService genreService)
        {
            _genreService = genreService;
        }

        /// <summary>Daftar genre — dipakai filter Explore & form EO, jadi tetap publik.</summary>
        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _genreService.GetAllAsync();
            return Ok(genres);
        }

        // ── Kelola genre (§4.4 Admin) ─────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGenre([FromBody] GenreRequest? request)
        {
            if (request == null)
                return BadRequest(new { message = "Body genre wajib dikirim" });

            var (genre, error) = await _genreService.CreateAsync(request.Name, request.Icon);
            if (genre == null)
                return BadRequest(new { message = error ?? "Gagal menambah genre" });

            return StatusCode(201, new { message = "Genre ditambahkan", genre });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreRequest? request)
        {
            if (request == null)
                return BadRequest(new { message = "Body genre wajib dikirim" });

            var (genre, error) = await _genreService.UpdateAsync(id, request.Name, request.Icon);
            if (genre == null)
                return BadRequest(new { message = error ?? "Gagal memperbarui genre" });

            return Ok(new { message = "Genre diperbarui", genre });
        }

        /// <summary>Hapus genre. UI wajib konfirmasi dulu (operasi destruktif §23).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var (success, error) = await _genreService.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = error ?? "Gagal menghapus genre" });

            return Ok(new { message = "Genre dihapus", id });
        }
    }

    public class GenreRequest
    {
        public string? Name { get; set; }
        public string? Icon { get; set; }
    }
}
