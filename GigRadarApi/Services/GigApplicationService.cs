using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Services;

/// <summary>
/// Lamaran artist untuk sebuah Gig (alur "Apply → Shortlist/Reject → Withdraw").
/// Validasi status, kepemilikan lamaran, dan kepemilikan event dilakukan
/// server-side — klaim dari client tidak pernah dipercaya (§16 research).
/// Status: PENDING | SHORTLISTED | REJECTED | WITHDRAWN.
/// </summary>
public class GigApplicationService(AppDbContext context, NotificationService notifications)
{
    public static readonly string[] AllowedStatuses = ["PENDING", "SHORTLISTED", "REJECTED", "WITHDRAWN"];

    // ── Artist: melamar gig ─────────────────────────────

    public async Task<(GigApplication? Application, string? Error, bool Forbidden, bool NotFound)> ApplyAsync(
        int gigId, int userId, ApplyToGigRequest request)
    {
        var artistId = await context.Artists.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.ArtistId)
            .FirstOrDefaultAsync();
        if (artistId == null)
            return (null, "Profil artist belum ada", true, false);

        var gig = await context.Gigs
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.GigId == gigId);
        if (gig == null)
            return (null, "Gig tidak ditemukan", false, true);

        if (gig.Status != "OPEN")
            return (null, "Gig ini belum dibuka atau sudah ditutup untuk lamaran.", false, false);
        if (gig.EndAt < DateTime.UtcNow)
            return (null, "Gig ini sudah lewat.", false, false);

        var message = request.Message?.Trim() ?? string.Empty;
        if (message.Length > 1000)
            return (null, "Pesan lamaran maksimal 1000 karakter.", false, false);

        var existing = await context.GigApplications
            .FirstOrDefaultAsync(a => a.GigId == gigId && a.ArtistId == artistId.Value);

        if (existing != null && existing.Status != "WITHDRAWN")
            return (null, "Kamu sudah melamar gig ini.", false, false);

        GigApplication application;
        if (existing != null)
        {
            // Lamaran yang pernah ditarik boleh diajukan ulang (unique index tetap terjaga).
            existing.Status = "PENDING";
            existing.Message = message;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.RespondedAt = null;
            application = existing;
        }
        else
        {
            application = new GigApplication
            {
                GigId = gigId,
                ArtistId = artistId.Value,
                Message = message,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };
            context.GigApplications.Add(application);
        }

        await context.SaveChangesAsync();

        // Notifikasi ke EO pemilik event (latar belakang notifikasi tetap tersimpan).
        var artistName = await context.Artists.AsNoTracking()
            .Where(a => a.ArtistId == artistId.Value)
            .Select(a => a.Name)
            .FirstOrDefaultAsync() ?? "Artist";
        await notifications.NotifyAsync(
            gig.Event!.CreatedBy,
            "GIG_APPLICATION",
            "Lamaran gig baru",
            $"\"{artistName}\" melamar gig \"{gig.Name}\" pada event \"{gig.Event.Name}\".",
            $"events/{gig.EventId}/gigs");

        return (application, null, false, false);
    }

    /// <summary>Lamaran milik artist yang login, terbaru dulu.</summary>
    public async Task<List<GigApplication>> GetMyAsync(int userId)
    {
        var artistId = await context.Artists.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.ArtistId)
            .FirstOrDefaultAsync();
        if (artistId == null)
            return new();

        return await context.GigApplications.AsNoTracking()
            .Where(a => a.ArtistId == artistId.Value)
            .Include(a => a.Gig).ThenInclude(g => g!.Event)
            .Include(a => a.Gig).ThenInclude(g => g!.Venue)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Artist menarik lamaran yang belum diputuskan.</summary>
    public async Task<(bool Success, string Message, bool Forbidden, bool NotFound)> WithdrawAsync(
        int applicationId, int userId)
    {
        var artistId = await context.Artists.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.ArtistId)
            .FirstOrDefaultAsync();

        var application = await context.GigApplications
            .Include(a => a.Gig).ThenInclude(g => g!.Event)
            .FirstOrDefaultAsync(a => a.GigApplicationId == applicationId);
        if (application == null)
            return (false, "Lamaran tidak ditemukan", false, true);

        if (artistId == null || application.ArtistId != artistId.Value)
            return (false, "Lamaran ini bukan milik kamu", true, false);

        if (application.Status is not ("PENDING" or "SHORTLISTED"))
            return (false, "Lamaran ini sudah diputuskan dan tidak bisa ditarik", false, false);

        application.Status = "WITHDRAWN";
        application.UpdatedAt = DateTime.UtcNow;
        application.RespondedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        if (application.Gig?.Event != null)
        {
            await notifications.NotifyAsync(
                application.Gig.Event.CreatedBy,
                "GIG_APPLICATION_UPDATE",
                "Lamaran ditarik",
                $"Seorang artist menarik lamaran untuk gig \"{application.Gig.Name}\".",
                $"events/{application.Gig.EventId}/gigs");
        }

        return (true, "Lamaran ditarik", false, false);
    }

    // ── EO: meninjau lamaran ────────────────────────────

    public async Task<(List<GigApplication>? Items, string? Error, bool Forbidden, bool NotFound)> GetForGigAsync(
        int gigId, int userId, string role)
    {
        var gig = await context.Gigs
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.GigId == gigId);
        if (gig == null)
            return (null, "Gig tidak ditemukan", false, true);
        if (!CanManage(gig.Event!, userId, role))
            return (null, "Kamu tidak memiliki akses untuk gig ini", true, false);

        var items = await context.GigApplications.AsNoTracking()
            .Where(a => a.GigId == gigId)
            .Include(a => a.Artist)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
        return (items, null, false, false);
    }

    /// <summary>EO shortlist atau menolak lamaran pada event miliknya.</summary>
    public async Task<(GigApplication? Application, string? Error, bool Forbidden, bool NotFound)> DecideAsync(
        int applicationId, int userId, string role, bool shortlist)
    {
        var application = await context.GigApplications
            .Include(a => a.Artist)
            .Include(a => a.Gig).ThenInclude(g => g!.Event)
            .FirstOrDefaultAsync(a => a.GigApplicationId == applicationId);
        if (application == null)
            return (null, "Lamaran tidak ditemukan", false, true);

        if (!CanManage(application.Gig!.Event!, userId, role))
            return (null, "Kamu tidak memiliki akses untuk lamaran ini", true, false);

        var target = shortlist ? "SHORTLISTED" : "REJECTED";

        if (shortlist && application.Status != "PENDING")
            return (null, "Hanya lamaran yang masih menunggu yang bisa di-shortlist", false, false);
        if (!shortlist && application.Status is not ("PENDING" or "SHORTLISTED"))
            return (null, "Lamaran ini sudah diputuskan", false, false);

        application.Status = target;
        application.UpdatedAt = DateTime.UtcNow;
        application.RespondedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Beri tahu artist pemilik lamaran (bila akun artist punya UserId).
        if (application.Artist?.UserId is int artistUserId)
        {
            await notifications.NotifyAsync(
                artistUserId,
                "GIG_APPLICATION_UPDATE",
                shortlist ? "Lamaran kamu di-shortlist" : "Lamaran kamu ditolak",
                shortlist
                    ? $"Lamaran kamu untuk gig \"{application.Gig.Name}\" masuk daftar kandidat."
                    : $"Lamaran kamu untuk gig \"{application.Gig.Name}\" belum berhasil.",
                $"events/{application.Gig.EventId}/gigs");
        }

        return (application, null, false, false);
    }

    private static bool CanManage(Event evt, int userId, string role) =>
        string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || evt.CreatedBy == userId;
}

public class ApplyToGigRequest
{
    public string Message { get; set; } = string.Empty;
}
