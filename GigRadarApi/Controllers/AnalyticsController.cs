using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;

namespace GigRadarApi.Controllers
{
    /// <summary>Statistik funnel satu event milik EO (Bab 69 — Conversion).</summary>
    public class EoEventAnalytics
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int Capacity { get; set; }
        public int TicketsSold { get; set; }
        public int RemainingStock { get; set; }
        public decimal Revenue { get; set; }
        public int TicketTypeCount { get; set; }
        public int LineupCount { get; set; }
        public int FavoritedCount { get; set; }
        /// <summary>Konversi = tiket terjual / kapasitas (0 bila kapasitas 0).</summary>
        public double ConversionRate { get; set; }
        public string RevenueFormatted => $"Rp {Revenue:N0}";
        public string ConversionLabel => $"{ConversionRate:P0}";
    }

    /// <summary>Statistik platform untuk Admin (Bab 69 — Admin).</summary>
    public class PlatformAnalytics
    {
        public int TotalUsers { get; set; }
        public int TotalArtists { get; set; }
        public int TotalEos { get; set; }
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalTickets { get; set; }
        public int PendingArtistVerifications { get; set; }
        public int PendingEoVerifications { get; set; }
        public int OpenReports { get; set; }
        public int TotalRevenue { get; set; }
        public string TotalRevenueFormatted => $"Rp {TotalRevenue:N0}";
    }

    /// <summary>
    /// Analytics (GIGRADAR_APP_SPEC.md Bab 69, STEP 21):
    /// - EO: funnel per event (terjual, revenue, konversi, favorit, line-up).
    /// - Admin: statistik platform (users, artists, EO, events, tiket, antrean verifikasi, laporan).
    /// </summary>
    [Route("api/analytics")]
    [ApiController]
    [Authorize]
    public class AnalyticsController(AppDbContext context) : ControllerBase
    {
        private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);

        /// <summary>Analytics event milik EO yang login (Bab 69 — EO).</summary>
        [HttpGet("eo")]
        [Authorize(Roles = "EO,Admin")]
        public async Task<IActionResult> GetEoAnalytics()
        {
            var userId = CurrentUserId;
            var isAdmin = User.IsInRole("Admin");

            var events = context.Events.AsQueryable();
            if (!isAdmin) events = events.Where(e => e.CreatedBy == userId);

            var list = await events
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EoEventAnalytics
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Status = e.Status,
                    StartDate = e.StartDate,
                    Capacity = e.Capacity,
                    TicketTypeCount = context.EventTicketTypes.Count(tt => tt.EventId == e.EventId),
                    LineupCount = e.EventArtists.Count,
                    FavoritedCount = e.Favorites.Count,
                })
                .ToListAsync();

            // TicketsSold & Revenue dihitung dari tiket terjual (Ticket.EventId langsung).
            var eventIds = list.Select(x => x.EventId).ToList();
            var sales = await context.Tickets
                .Where(t => eventIds.Contains(t.EventId))
                .GroupBy(t => t.EventId)
                .Select(g => new { EventId = g.Key, Count = g.Count(), Sum = g.Sum(x => x.Price) })
                .ToListAsync();

            foreach (var item in list)
            {
                var sale = sales.FirstOrDefault(s => s.EventId == item.EventId);
                item.TicketsSold = sale?.Count ?? 0;
                item.Revenue = sale?.Sum ?? 0;
                item.RemainingStock = Math.Max(0, item.Capacity - item.TicketsSold);
                item.ConversionRate = item.Capacity > 0 ? (double)item.TicketsSold / item.Capacity : 0;
            }

            return Ok(list);
        }

        /// <summary>Statistik platform (Bab 69 — Admin).</summary>
        [HttpGet("platform")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlatformAnalytics()
        {
            var stats = new PlatformAnalytics
            {
                TotalUsers = await context.Users.CountAsync(u => u.Role == "User"),
                TotalArtists = await context.Artists.CountAsync(),
                TotalEos = await context.Organizers.CountAsync(),
                TotalEvents = await context.Events.CountAsync(),
                ActiveEvents = await context.Events.CountAsync(e => e.Status == "Published" && e.EndDate >= DateTime.UtcNow),
                TotalTickets = await context.Tickets.CountAsync(),
                PendingArtistVerifications = await context.ArtistVerifications.CountAsync(v => v.Status == "PENDING"),
                PendingEoVerifications = await context.OrganizerVerifications.CountAsync(v => v.Status == "PENDING"),
                OpenReports = await context.Reports.CountAsync(r => r.Status == "OPEN"),
                TotalRevenue = (int)await context.Tickets.SumAsync(t => t.Price)
            };
            return Ok(stats);
        }
    }
}
