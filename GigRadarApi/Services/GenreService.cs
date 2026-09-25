using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Services
{
    /// <summary>
    /// Kelola data master Genre (§4.4 Admin: "Mengelola genre"; §23).
    /// Genre dipakai event (Event.GenreId) dan preferensi user (UserPreference.GenreId),
    /// jadi penghapusan harus sadar-pemakaian agar tidak memutus data yang valid.
    /// </summary>
    public class GenreService
    {
        private readonly AppDbContext _context;

        public GenreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Genre>> GetAllAsync()
        {
            return await _context.Genres
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<(Genre? Genre, string? Error)> CreateAsync(string? name, string? icon)
        {
            var trimmedName = (name ?? string.Empty).Trim();
            var error = ValidateName(trimmedName);
            if (error != null) return (null, error);

            if (await _context.Genres.AnyAsync(g => g.Name.ToLower() == trimmedName.ToLower()))
                return (null, "Genre dengan nama itu sudah ada");

            var genre = new Genre
            {
                Name = trimmedName,
                Icon = (icon ?? string.Empty).Trim()
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return (genre, null);
        }

        public async Task<(Genre? Genre, string? Error)> UpdateAsync(int id, string? name, string? icon)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return (null, "Genre tidak ditemukan");

            var trimmedName = (name ?? string.Empty).Trim();
            var error = ValidateName(trimmedName);
            if (error != null) return (null, error);

            if (await _context.Genres.AnyAsync(g => g.GenreId != id && g.Name.ToLower() == trimmedName.ToLower()))
                return (null, "Genre dengan nama itu sudah ada");

            genre.Name = trimmedName;
            genre.Icon = (icon ?? string.Empty).Trim();

            await _context.SaveChangesAsync();
            return (genre, null);
        }

        /// <summary>
        /// Hapus genre. Ditolak bila masih dipakai event atau preferensi user — penghapusan
        /// paksa akan menghasilkan event tanpa genre atau preferensi menggantung.
        /// </summary>
        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return (false, "Genre tidak ditemukan");

            var usedByEvents = await _context.Events.CountAsync(e => e.GenreId == id);
            if (usedByEvents > 0)
                return (false, $"Genre \"{genre.Name}\" masih dipakai {usedByEvents} event. Pindahkan genre event tersebut dulu.");

            var usedByPreferences = await _context.UserPreferences.CountAsync(p => p.GenreId == id);
            if (usedByPreferences > 0)
                return (false, $"Genre \"{genre.Name}\" masih jadi preferensi {usedByPreferences} user.");

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        private static string? ValidateName(string name)
        {
            if (name.Length == 0) return "Nama genre wajib diisi";
            if (name.Length > 50) return "Nama genre maksimal 50 karakter";
            return null;
        }
    }
}
