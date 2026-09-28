using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Services;

/// <summary>
/// Gig = slot artist yang menempel pada event EO (§5 dokumen research).
/// Mengelola CRUD gig oleh EO pemilik event serta penemuan gig terbuka
/// untuk artist. Semua mutation memvalidasi kepemilikan event server-side.
/// </summary>
public class GigService(AppDbContext context)
{
    public static readonly string[] AllowedStatuses = ["DRAFT", "OPEN", "CLOSED"];

    /// <summary>Transisi status yang sah — divalidasi server-side, bukan di client.</summary>
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        ["DRAFT"] = ["OPEN"],
        ["OPEN"] = ["CLOSED"],
        ["CLOSED"] = ["OPEN"]
    };

    // ── EO: kelola gig milik event sendiri ──────────────

    public async Task<(List<Gig>? Gigs, string? Error, bool Forbidden)> GetForEventAsync(
        int eventId, int userId, string role)
    {
        var evt = await context.Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);
        if (evt == null)
            return (null, "Event tidak ditemukan", false);
        if (!CanManage(evt, userId, role))
            return (null, "Kamu tidak memiliki akses untuk mengelola event ini", true);

        var gigs = await context.Gigs
            .Where(g => g.EventId == eventId)
            .Include(g => g.Venue)
            .Include(g => g.Genre)
            .Include(g => g.Applications)
            .OrderBy(g => g.StartAt)
            .ToListAsync();
        return (gigs, null, false);
    }

    public async Task<(Gig? Gig, string? Error, bool Forbidden, bool NotFound)> CreateAsync(
        int eventId, int userId, string role, CreateGigRequest request)
    {
        var evt = await context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (evt == null)
            return (null, "Event tidak ditemukan", false, true);
        if (!CanManage(evt, userId, role))
            return (null, "Kamu tidak memiliki akses untuk mengelola event ini", true, false);

        var error = Validate(request, evt);
        if (error != null)
            return (null, error, false, false);

        var gig = new Gig
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            Requirements = request.Requirements?.Trim() ?? string.Empty,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            VenueId = evt.VenueId,
            GenreId = evt.GenreId,
            BudgetMin = request.BudgetMin,
            BudgetMax = request.BudgetMax,
            RequiredArtistCount = request.RequiredArtistCount,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        context.Gigs.Add(gig);
        await context.SaveChangesAsync();
        await context.Entry(gig).Reference(g => g.Venue).LoadAsync();
        await context.Entry(gig).Reference(g => g.Genre).LoadAsync();
        return (gig, null, false, false);
    }

    public async Task<(Gig? Gig, string? Error, bool Forbidden, bool NotFound)> UpdateAsync(
        int eventId, int gigId, int userId, string role, CreateGigRequest request)
    {
        var evt = await context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (evt == null)
            return (null, "Event tidak ditemukan", false, true);
        if (!CanManage(evt, userId, role))
            return (null, "Kamu tidak memiliki akses untuk mengelola event ini", true, false);

        var gig = await context.Gigs
            .Include(g => g.Venue)
            .Include(g => g.Genre)
            .FirstOrDefaultAsync(g => g.GigId == gigId && g.EventId == eventId);
        if (gig == null)
            return (null, "Gig tidak ditemukan", false, true);

        var error = Validate(request, evt);
        if (error != null)
            return (null, error, false, false);

        // Status gig hanya boleh berubah lewat transisi yang sah.
        if (!string.Equals(gig.Status, request.Status, StringComparison.OrdinalIgnoreCase))
        {
            var transitionError = CheckTransition(gig.Status, request.Status);
            if (transitionError != null)
                return (null, transitionError, false, false);
        }

        gig.Name = request.Name.Trim();
        gig.Requirements = request.Requirements?.Trim() ?? string.Empty;
        gig.StartAt = request.StartAt;
        gig.EndAt = request.EndAt;
        gig.BudgetMin = request.BudgetMin;
        gig.BudgetMax = request.BudgetMax;
        gig.RequiredArtistCount = request.RequiredArtistCount;
        gig.Status = request.Status;
        gig.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return (gig, null, false, false);
    }

    /// <summary>Mengubah status gig lewat lifecycle DRAFT ⇄ OPEN → CLOSED.</summary>
    public async Task<(Gig? Gig, string? Error, bool Forbidden, bool NotFound)> ChangeStatusAsync(
        int eventId, int gigId, int userId, string role, string targetStatus)
    {
        var evt = await context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (evt == null)
            return (null, "Event tidak ditemukan", false, true);
        if (!CanManage(evt, userId, role))
            return (null, "Kamu tidak memiliki akses untuk mengelola event ini", true, false);

        var gig = await context.Gigs
            .Include(g => g.Venue)
            .Include(g => g.Genre)
            .FirstOrDefaultAsync(g => g.GigId == gigId && g.EventId == eventId);
        if (gig == null)
            return (null, "Gig tidak ditemukan", false, true);

        var target = (targetStatus ?? string.Empty).Trim().ToUpperInvariant();
        if (!AllowedStatuses.Contains(target))
            return (null, "Status gig tidak dikenal.", false, false);

        var transitionError = CheckTransition(gig.Status, target);
        if (transitionError != null)
            return (null, transitionError, false, false);

        gig.Status = target;
        gig.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return (gig, null, false, false);
    }

    public async Task<(bool Success, string Message, bool Forbidden, bool NotFound)> DeleteAsync(
        int eventId, int gigId, int userId, string role)
    {
        var evt = await context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (evt == null)
            return (false, "Event tidak ditemukan", false, true);
        if (!CanManage(evt, userId, role))
            return (false, "Kamu tidak memiliki akses untuk mengelola event ini", true, false);

        var gig = await context.Gigs.FirstOrDefaultAsync(g => g.GigId == gigId && g.EventId == eventId);
        if (gig == null)
            return (false, "Gig tidak ditemukan", false, true);

        // Lamaran ikut terhapus (cascade aplikasi level + eksplisit agar deterministik).
        var applications = await context.GigApplications.Where(a => a.GigId == gigId).ToListAsync();
        context.GigApplications.RemoveRange(applications);
        context.Gigs.Remove(gig);
        await context.SaveChangesAsync();
        return (true, $"Gig \"{gig.Name}\" dihapus", false, false);
    }

    // ── Artist: penemuan gig terbuka ────────────────────

    /// <summary>
    /// Daftar gig OPEN pada event yang belum lewat, untuk halaman Discover artist.
    /// Filter: q (nama gig/event/venue/kota), genreId, maxBudget, city.
    /// </summary>
    public async Task<List<GigResponse>> GetOpenGigsAsync(
        int userId, string? q = null, int? genreId = null, decimal? maxBudget = null, string? city = null)
    {
        var artistId = await GetArtistIdAsync(userId);

        var query = context.Gigs.AsNoTracking()
            .Include(g => g.Venue)
            .Include(g => g.Genre)
            .Include(g => g.Event)
            .Where(g => g.Status == "OPEN" && g.Event != null && g.EndAt >= DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(g =>
                g.Name.ToLower().Contains(term.ToLower()) ||
                g.Event!.Name.ToLower().Contains(term.ToLower()) ||
                (g.Venue != null && g.Venue.Name.ToLower().Contains(term.ToLower())) ||
                (g.Venue != null && g.Venue.City.ToLower().Contains(term.ToLower())));
        }

        if (genreId.HasValue)
            query = query.Where(g => g.GenreId == genreId.Value);

        if (maxBudget.HasValue)
            query = query.Where(g => g.BudgetMin <= maxBudget.Value);

        if (!string.IsNullOrWhiteSpace(city))
        {
            var cityTerm = city.Trim();
            query = query.Where(g => g.Venue != null && g.Venue.City.ToLower().Contains(cityTerm.ToLower()));
        }

        var gigs = await query
            .OrderBy(g => g.StartAt)
            .Take(100)
            .ToListAsync();

        var gigIds = gigs.Select(g => g.GigId).ToList();

        var counts = await context.GigApplications
            .Where(a => gigIds.Contains(a.GigId))
            .GroupBy(a => a.GigId)
            .Select(group => new { GigId = group.Key, Count = group.Count() })
            .ToListAsync();
        var countLookup = counts.ToDictionary(c => c.GigId, c => c.Count);

        var appliedGigIds = artistId == null
            ? new HashSet<int>()
            : (await context.GigApplications
                .Where(a => a.ArtistId == artistId && gigIds.Contains(a.GigId) && a.Status != "WITHDRAWN")
                .Select(a => a.GigId)
                .ToListAsync()).ToHashSet();

        return gigs.Select(g => GigResponse.From(
            g,
            countLookup.GetValueOrDefault(g.GigId),
            appliedGigIds.Contains(g.GigId))).ToList();
    }

    // ── Helper ──────────────────────────────────────────

    /// <summary>Profil artist milik user login (null bila user belum punya profil artist).</summary>
    public async Task<int?> GetArtistIdAsync(int userId) =>
        await context.Artists.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.ArtistId)
            .FirstOrDefaultAsync();

    public static string? CheckTransition(string current, string target)
    {
        if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase))
            return $"Gig sudah berstatus {target}.";

        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target)
            ? null
            : $"Status gig tidak bisa berubah dari {current} ke {target}.";
    }

    public static string? Validate(CreateGigRequest request, Event evt)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 3)
            return "Nama gig wajib diisi (minimal 3 karakter).";
        if (request.Name.Trim().Length > 120)
            return "Nama gig maksimal 120 karakter.";
        if (request.Requirements?.Length > 2000)
            return "Persyaratan maksimal 2000 karakter.";
        if (request.EndAt <= request.StartAt)
            return "Waktu selesai gig harus setelah waktu mulai.";
        if (request.StartAt < evt.StartDate || request.EndAt > evt.EndDate)
            return "Jadwal gig harus berada di dalam jadwal event.";
        if (!evt.VenueId.HasValue)
            return "Event harus memiliki venue sebelum gig dapat dibuat.";
        if (request.BudgetMin < 0 || request.BudgetMax < 0)
            return "Budget tidak boleh bernilai negatif.";
        if (request.BudgetMin > request.BudgetMax)
            return "Budget minimum tidak boleh lebih besar dari budget maksimum.";
        if (request.RequiredArtistCount is < 1 or > 20)
            return "Jumlah artist yang dibutuhkan harus antara 1 dan 20.";
        if (request.Status is not ("DRAFT" or "OPEN"))
            return "Status gig harus DRAFT atau OPEN.";

        return null;
    }

    private static bool CanManage(Event evt, int userId, string role) =>
        string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || evt.CreatedBy == userId;
}

public class CreateGigRequest
{
    public string Name { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int RequiredArtistCount { get; set; } = 1;
    public string Status { get; set; } = "OPEN";
}

/// <summary>
/// Kontrak response gig — dipakai bersama oleh endpoint EO (kelola) dan
/// Discover artist. Tipe nyata (bukan anonymous object) agar kontrak stabil
/// dan bisa diverifikasi test.
/// </summary>
public class GigResponse
{
    public int GigId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int? VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int RequiredArtistCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime? EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public int ApplicantCount { get; set; }
    public bool HasApplied { get; set; }

    public static GigResponse From(Gig gig, int applicantCount = 0, bool hasApplied = false) => new()
    {
        GigId = gig.GigId,
        EventId = gig.EventId,
        Name = gig.Name,
        Requirements = gig.Requirements,
        StartAt = gig.StartAt,
        EndAt = gig.EndAt,
        VenueId = gig.VenueId,
        VenueName = gig.Venue?.Name ?? string.Empty,
        City = gig.Venue?.City ?? string.Empty,
        GenreId = gig.GenreId,
        GenreName = gig.Genre?.Name ?? string.Empty,
        BudgetMin = gig.BudgetMin,
        BudgetMax = gig.BudgetMax,
        RequiredArtistCount = gig.RequiredArtistCount,
        Status = gig.Status,
        CreatedAt = gig.CreatedAt,
        UpdatedAt = gig.UpdatedAt,
        EventName = gig.Event?.Name ?? string.Empty,
        EventStartDate = gig.Event?.StartDate,
        EventEndDate = gig.Event?.EndDate,
        ApplicantCount = applicantCount,
        HasApplied = hasApplied
    };
}
