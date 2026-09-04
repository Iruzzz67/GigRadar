using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Services
{
    public class EventService
    {
        private readonly AppDbContext _context;

        /// <summary>Status event yang diperbolehkan.</summary>
        public static readonly string[] AllowedStatuses = { "Published", "Draft", "SoldOut", "Completed" };

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        // ── Read ─────────────────────────────────────────

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<List<Event>> GetNearbyEventsAsync(double lat, double lng, double radiusKm = 50)
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .Where(e => e.Status == "Published")
                .ToListAsync();

            return events
                .Where(e => CalculateDistance(lat, lng, e.Latitude, e.Longitude) <= radiusKm)
                .OrderBy(e => CalculateDistance(lat, lng, e.Latitude, e.Longitude))
                .ToList();
        }

        public async Task<List<Event>> GetRecommendedEventsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Preferences)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return await GetAllEventsAsync();

            var preferredGenreIds = user.Preferences.Select(p => p.GenreId).ToList();

            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .Where(e => e.Status == "Published")
                .ToListAsync();

            // Rule-based recommendation scoring
            var scored = events.Select(e =>
            {
                var genreScore = e.GenreId.HasValue && preferredGenreIds.Contains(e.GenreId.Value) ? 0.4 : 0.0;
                var locationScore = CalculateDistance(user.Latitude, user.Longitude, e.Latitude, e.Longitude) < 20 ? 0.3 : 0.1;
                var popularityScore = Math.Min(e.ViewsCount / 100.0, 1.0) * 0.1;
                var recencyScore = (e.StartDate - DateTime.UtcNow).TotalDays < 7 ? 0.2 : 0.1;

                return new { Event = e, Score = genreScore + locationScore + popularityScore + recencyScore };
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Event)
            .ToList();

            return scored;
        }

        /// <summary>
        /// Event yang dikelola pengguna untuk halaman admin/EO.
        /// Admin melihat semua event; EO hanya event yang ia buat (CreatedBy).
        /// </summary>
        public async Task<List<Event>> GetManagedEventsAsync(int userId, string role)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .AsQueryable();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                query = query.Where(e => e.CreatedBy == userId);

            return await query.OrderByDescending(e => e.StartDate).ToListAsync();
        }

        /// <summary>
        /// Ringkasan untuk halaman profil EO: statistik agregat (total event, event
        /// mendatang, tiket terjual, pendapatan) plus statistik per event
        /// (tiket terjual, revenue, jumlah tipe tiket, sisa stok).
        /// </summary>
        public async Task<object> GetManagedSummaryAsync(int userId, string role)
        {
            var events = await GetManagedEventsAsync(userId, role);
            var eventIds = events.Select(e => e.EventId).ToList();

            // Agregasi dilakukan di sisi klien (dataset kecil) — SQLite tidak bisa
            // menerjemahkan GroupBy + Sum(decimal) ke SQL.
            var tickets = await _context.Tickets
                .Where(t => eventIds.Contains(t.EventId) && t.Status != "Cancelled")
                .Select(t => new { t.EventId, t.Price })
                .ToListAsync();

            var ticketStats = tickets
                .GroupBy(t => t.EventId)
                .Select(g => new { EventId = g.Key, Sold = g.Count(), Revenue = g.Sum(t => t.Price) })
                .ToList();

            var types = await _context.EventTicketTypes
                .Where(t => eventIds.Contains(t.EventId))
                .Select(t => new { t.EventId, t.Stock })
                .ToListAsync();

            var typeStats = types
                .GroupBy(t => t.EventId)
                .Select(g => new { EventId = g.Key, Types = g.Count(), Stock = g.Sum(t => t.Stock) })
                .ToList();

            var now = DateTime.UtcNow;

            var eventStats = events.Select(e =>
            {
                var ts = ticketStats.FirstOrDefault(x => x.EventId == e.EventId);
                var tp = typeStats.FirstOrDefault(x => x.EventId == e.EventId);
                return new
                {
                    eventId = e.EventId,
                    name = e.Name,
                    status = e.Status,
                    startDate = e.StartDate,
                    capacity = e.Capacity,
                    ticketsSold = ts?.Sold ?? 0,
                    revenue = ts?.Revenue ?? 0m,
                    ticketTypeCount = tp?.Types ?? 0,
                    remainingStock = tp?.Stock ?? 0
                };
            }).ToList();

            return new
            {
                totalEvents = events.Count,
                upcomingEvents = events.Count(e => e.StartDate >= now),
                totalTicketsSold = eventStats.Sum(e => e.ticketsSold),
                totalRevenue = eventStats.Sum(e => e.revenue),
                events = eventStats
            };
        }

        public async Task<List<Event>> GetTonightEventsAsync()
        {
            var today = DateTime.Today;
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .Where(e => e.StartDate.Date == today && e.Status == "Published")
                .ToListAsync();
        }

        public async Task<List<Event>> GetWeekendEventsAsync()
        {
            var today = DateTime.Today;
            var weekendEnd = today.AddDays(7 - (int)today.DayOfWeek);
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Genre)
                .Include(e => e.EventArtists).ThenInclude(ea => ea.Artist)
                .Where(e => e.StartDate >= today && e.StartDate <= weekendEnd && e.Status == "Published")
                .ToListAsync();
        }

        // ── Create ────────────────────────────────────────

        public async Task<Event?> CreateEventAsync(Event evt)
        {
            // Field yang tidak boleh diatur dari body request:
            evt.EventId = 0;
            evt.CreatedAt = DateTime.UtcNow;
            evt.ViewsCount = 0;
            evt.SavesCount = 0;

            // Navigation object dari request diabaikan — hanya ID yang dipakai,
            // agar EF tidak mencoba menyimpan Venue/Genre/User baru.
            evt.Venue = null;
            evt.Genre = null;
            evt.Organizer = null;

            evt.TicketLink = evt.TicketLink?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(evt.Status))
                evt.Status = "Published";

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(evt.EventId);
        }

        // ── Update ────────────────────────────────────────

        /// <summary>
        /// Update data inti event (PUT). Admin boleh semua; EO hanya event miliknya.
        /// </summary>
        public async Task<(bool Success, string? Error, bool Forbidden)> UpdateEventAsync(
            int id, Event evt, int userId, string role)
        {
            var entity = await _context.Events.FindAsync(id);
            if (entity == null) return (false, "Event tidak ditemukan", false);

            if (!await CanManageEventAsync(id, userId, role))
                return (false, "Kamu tidak memiliki akses untuk mengelola event ini", true);

            // Field mutable — PUT bersifat replace untuk data inti event.
            entity.Name = evt.Name;
            entity.Description = evt.Description ?? string.Empty;
            entity.PosterUrl = evt.PosterUrl ?? string.Empty;
            entity.TicketLink = evt.TicketLink ?? string.Empty;
            entity.VenueId = evt.VenueId;
            entity.GenreId = evt.GenreId;
            entity.StartDate = evt.StartDate;
            entity.EndDate = evt.EndDate;
            entity.Latitude = evt.Latitude;
            entity.Longitude = evt.Longitude;
            entity.MinPrice = evt.MinPrice;
            entity.MaxPrice = evt.MaxPrice;
            entity.Capacity = evt.Capacity;
            entity.Status = string.IsNullOrWhiteSpace(evt.Status) ? entity.Status : evt.Status;

            // Line-up (EventArtists) hanya di-replace bila benar-benar dikirim pada body.
            var incomingArtists = evt.EventArtists;
            if (incomingArtists is { Count: > 0 })
            {
                var existing = await _context.EventArtists
                    .Where(ea => ea.EventId == id)
                    .ToListAsync();

                _context.EventArtists.RemoveRange(existing);

                var order = 0;
                foreach (var ea in incomingArtists)
                {
                    if (ea.ArtistId <= 0) continue;
                    order = ea.Order > 0 ? ea.Order : order + 1;
                    _context.EventArtists.Add(new EventArtist { EventId = id, ArtistId = ea.ArtistId, Order = order });
                }
            }

            await _context.SaveChangesAsync();
            return (true, null, false);
        }

        /// <summary>
        /// Ubah status event: Published (aktif), Draft, SoldOut (tiket habis),
        /// Completed (event selesai). Penjualan tiket hanya untuk Published.
        /// </summary>
        public async Task<(Event? Event, string? Error, bool Forbidden)> ChangeEventStatusAsync(
            int id, int userId, string role, string status)
        {
            var entity = await _context.Events.FindAsync(id);
            if (entity == null) return (null, "Event tidak ditemukan", false);

            if (!await CanManageEventAsync(id, userId, role))
                return (null, "Kamu tidak memiliki akses untuk mengelola event ini", true);

            var normalized = status?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
                return (null, "Status wajib diisi", false);

            if (!AllowedStatuses.Contains(normalized))
                return (null, $"Status tidak valid. Gunakan: {string.Join(", ", AllowedStatuses)}", false);

            entity.Status = normalized;
            await _context.SaveChangesAsync();

            return (await GetEventByIdAsync(id), null, false);
        }

        // ── Delete ────────────────────────────────────────

        /// <summary>
        /// Hapus event beserta relasinya. Admin boleh menghapus semua event;
        /// EO hanya event yang ia buat.
        /// </summary>
        public async Task<(bool Deleted, string? Error, bool Forbidden)> DeleteEventAsync(
            int id, int userId, string role)
        {
            var entity = await _context.Events.FindAsync(id);
            if (entity == null) return (false, "Event tidak ditemukan", false);

            if (!await CanManageEventAsync(id, userId, role))
                return (false, "Kamu tidak memiliki akses untuk mengelola event ini", true);

            // Hapus data terkait secara eksplisit agar tidak ada FK yang menggantung
            // (line-up, favorit, dan tiket milik event ini).
            var eventArtists = await _context.EventArtists.Where(ea => ea.EventId == id).ToListAsync();
            var favorites = await _context.Favorites.Where(f => f.EventId == id).ToListAsync();
            var tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync();

            _context.EventArtists.RemoveRange(eventArtists);
            _context.Favorites.RemoveRange(favorites);
            _context.Tickets.RemoveRange(tickets);
            _context.Events.Remove(entity);

            await _context.SaveChangesAsync();
            return (true, null, false);
        }

        /// <summary>Admin boleh kelola semua event; EO hanya event yang ia buat (CreatedBy).</summary>
        private async Task<bool> CanManageEventAsync(int eventId, int userId, string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            var evt = await _context.Events.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            return evt != null && evt.CreatedBy == userId;
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
