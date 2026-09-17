using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Services;

/// <summary>Body POST /api/organizer/verification (Bab 13 — EO Registration).</summary>
public class OrganizerVerificationRequest
{
    public string? Contact { get; set; }
}

/// <summary>Body POST /api/organizer/portfolio (Bab 14).</summary>
public class PortfolioItemRequest
{
    public string? EventName { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Venue { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public string? PhotosUrl { get; set; }
    public string? DocumentationLink { get; set; }
}

/// <summary>
/// Logika bisnis verifikasi EO (GIGRADAR_APP_SPEC.md Bab 12–15):
/// - EO kelola profil organizer + portofolio event sebelumnya (bukti).
/// - Submit verifikasi butuh profil lengkap + minimal satu portofolio (Bab 13).
/// - Hanya Admin yang dapat approve/reject — status profil server-controlled.
/// - Setiap keputusan tercatat (ReviewedBy/RejectedReason/ReviewedAt) sebagai jejak audit.
/// - Pengajuan yang ditolak boleh diajukan ulang (satu PENDING per organizer).
/// </summary>
public class OrganizerVerificationService(AppDbContext context, NotificationService notifications)
{
    /// <summary>Filter status yang valid untuk query daftar pengajuan.</summary>
    public static readonly string[] AllowedStatusFilters = ["PENDING", "VERIFIED", "REJECTED"];

    /// <summary>Profil organizer milik user yang login, atau null.</summary>
    public Task<Organizer?> GetOwnOrganizerAsync(int userId) =>
        context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);

    /// <summary>
    /// Profil organizer milik user — dibuat otomatis bila belum ada (dari data User),
    /// agar EO baru bisa langsung melengkapi profil & portofolio (Bab 13).
    /// </summary>
    public async Task<Organizer> GetOrCreateOwnOrganizerAsync(int userId)
    {
        var organizer = await context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
        if (organizer != null) return organizer;

        var user = await context.Users.FindAsync(userId);
        organizer = new Organizer
        {
            UserId = userId,
            OrganizationName = user?.Name ?? "Organizer",
            Location = user?.City ?? "",
            Contact = ""
        };
        context.Organizers.Add(organizer);
        await context.SaveChangesAsync();
        return organizer;
    }

    /// <summary>Perbarui profil organizer milik user yang login (Bab 12 — Organizer Profile).</summary>
    public async Task<(bool Success, string Message)> UpdateProfileAsync(
        int userId, string organizationName, string description, string location,
        string contact, string websiteUrl, string socialLinks, int? foundedYear)
    {
        if (string.IsNullOrWhiteSpace(organizationName))
            return (false, "Nama organisasi wajib diisi");

        var organizer = await GetOrCreateOwnOrganizerAsync(userId);
        organizer.OrganizationName = organizationName.Trim();
        organizer.Description = description?.Trim() ?? "";
        organizer.Location = location?.Trim() ?? "";
        organizer.Contact = contact?.Trim() ?? "";
        organizer.WebsiteUrl = websiteUrl?.Trim() ?? "";
        organizer.SocialLinks = socialLinks?.Trim() ?? "";
        organizer.FoundedYear = foundedYear;

        // Mengubah profil saat PENDING mengembalikan status ke NOT_SUBMITTED agar
        // Admin meninjau data terbaru, bukan snapshot lama (integritas review).
        if (organizer.VerificationStatus == "PENDING")
            organizer.VerificationStatus = "NOT_SUBMITTED";

        await context.SaveChangesAsync();
        return (true, "Profil organizer tersimpan");
    }

    /// <summary>Tambah item portofolio milik organizer yang login (Bab 14).</summary>
    public async Task<(bool Success, string Message)> AddPortfolioItemAsync(
        int userId, PortfolioItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
            return (false, "Nama event wajib diisi");

        var hasDescriptionOrLink = !string.IsNullOrWhiteSpace(request.Description)
            || !string.IsNullOrWhiteSpace(request.DocumentationLink)
            || !string.IsNullOrWhiteSpace(request.PhotosUrl);
        if (!hasDescriptionOrLink)
            return (false, "Sertakan deskripsi, foto, atau link dokumentasi sebagai bukti");

        var organizer = await GetOrCreateOwnOrganizerAsync(userId);
        context.OrganizerPortfolioItems.Add(new OrganizerPortfolioItem
        {
            OrganizerId = organizer.OrganizerId,
            EventName = request.EventName.Trim(),
            EventDate = request.EventDate,
            Venue = request.Venue?.Trim() ?? "",
            Description = request.Description?.Trim() ?? "",
            PosterUrl = request.PosterUrl?.Trim() ?? "",
            PhotosUrl = request.PhotosUrl?.Trim() ?? "",
            DocumentationLink = request.DocumentationLink?.Trim() ?? ""
        });
        await context.SaveChangesAsync();
        return (true, $"Portofolio \"{request.EventName.Trim()}\" ditambahkan");
    }

    /// <summary>Hapus item portofolio milik sendiri (validasi ownership).</summary>
    public async Task<(bool Success, string Message)> DeletePortfolioItemAsync(int userId, int portfolioItemId)
    {
        var organizer = await GetOwnOrganizerAsync(userId);
        if (organizer == null) return (false, "Profil organizer belum ada");

        var item = await context.OrganizerPortfolioItems
            .FirstOrDefaultAsync(p => p.PortfolioItemId == portfolioItemId && p.OrganizerId == organizer.OrganizerId);
        if (item == null) return (false, "Item portofolio tidak ditemukan");

        context.OrganizerPortfolioItems.Remove(item);
        await context.SaveChangesAsync();
        return (true, "Item portofolio dihapus");
    }

    /// <summary>
    /// Ajukan verifikasi EO (Bab 13): profil wajib lengkap, minimal satu portofolio,
    /// contact wajib. Satu PENDING per organizer; setelah REJECTED boleh ajukan ulang.
    /// </summary>
    public async Task<(bool Success, string Message, object? Result)> SubmitVerificationAsync(
        int userId, OrganizerVerificationRequest request)
    {
        var organizer = await GetOrCreateOwnOrganizerAsync(userId);

        // Validasi kelengkapan profil (Bab 13: Organizer Profile → Portfolio → Submit).
        if (string.IsNullOrWhiteSpace(organizer.Description))
            return (false, "Lengkapi deskripsi organisasi dulu sebelum mengajukan verifikasi", null);
        if (string.IsNullOrWhiteSpace(request.Contact) || request.Contact.Trim().Length < 9)
            return (false, "Nomor telepon / kontak tidak valid (minimal 9 karakter)", null);
        if (string.IsNullOrWhiteSpace(organizer.Location))
            return (false, "Lengkapi lokasi organisasi dulu sebelum mengajukan verifikasi", null);

        var portfolio = await context.OrganizerPortfolioItems
            .Where(p => p.OrganizerId == organizer.OrganizerId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
        if (portfolio.Count == 0)
            return (false, "Tambahkan minimal satu portofolio event sebagai bukti pengalaman", null);

        var hasPending = await context.OrganizerVerifications
            .AnyAsync(v => v.OrganizerId == organizer.OrganizerId && v.Status == "PENDING");
        if (hasPending)
            return (false, "Kamu sudah punya pengajuan yang sedang ditinjau", null);

        var verification = new OrganizerVerification
        {
            OrganizerId = organizer.OrganizerId,
            OrganizationName = organizer.OrganizationName,
            Description = organizer.Description,
            Location = organizer.Location,
            WebsiteUrl = organizer.WebsiteUrl,
            Contact = request.Contact.Trim(),
            PortfolioCount = portfolio.Count,
            PortfolioSummary = string.Join(" | ", portfolio.Select(p =>
                $"{p.EventName}{(string.IsNullOrWhiteSpace(p.DocumentationLink) ? "" : $" — {p.DocumentationLink}")}")),
            Status = "PENDING",
            SubmittedAt = DateTime.UtcNow
        };

        context.OrganizerVerifications.Add(verification);
        organizer.VerificationStatus = "PENDING";
        organizer.IsVerified = false;

        await context.SaveChangesAsync();
        return (true, "Pengajuan verifikasi terkirim — menunggu tinjauan Admin", verification);
    }

    /// <summary>Keputusan Admin atas pengajuan verifikasi EO (Bab 15 — hanya Admin).</summary>
    public async Task<(bool Success, string Message)> ReviewAsync(
        int verificationId, int adminUserId, bool approve, string? reason)
    {
        var verification = await context.OrganizerVerifications
            .Include(v => v.Organizer)
            .FirstOrDefaultAsync(v => v.VerificationId == verificationId);

        if (verification == null)
            return (false, "Pengajuan verifikasi tidak ditemukan");

        if (verification.Status != "PENDING")
            return (false, "Pengajuan ini sudah diputuskan");

        if (verification.Organizer == null)
            return (false, "Organizer pemilik pengajuan tidak ditemukan");

        verification.Status = approve ? "VERIFIED" : "REJECTED";
        verification.ReviewedAt = DateTime.UtcNow;
        verification.ReviewedByUserId = adminUserId;

        // Notifikasi keputusan ke EO (Bab 36 — EO_VERIFICATION).
        if (verification.Organizer!.UserId != null)
        {
            await notifications.NotifyAsync(verification.Organizer.UserId.Value, "EO_VERIFICATION",
                approve ? "Verifikasi disetujui" : "Verifikasi ditolak",
                approve
                    ? $"Selamat! \"{verification.Organizer.OrganizationName}\" kini terverifikasi."
                    : $"Pengajuan verifikasi \"{verification.Organizer.OrganizationName}\" ditolak: {verification.RejectedReason}",
                "verification");
        }

        if (!approve)
        {
            verification.RejectedReason = string.IsNullOrWhiteSpace(reason)
                ? "Tidak memenuhi kriteria verifikasi"
                : reason.Trim();
        }

        // Status profil mengikuti keputusan terakhir (server-controlled, Bab 15).
        verification.Organizer.VerificationStatus = verification.Status;
        verification.Organizer.IsVerified = approve;

        await context.SaveChangesAsync();
        var msg = approve
            ? $"\"{verification.Organizer.OrganizationName}\" terverifikasi"
            : $"Pengajuan \"{verification.Organizer.OrganizationName}\" ditolak";
        return (true, msg);
    }

    /// <summary>Status verifikasi EO milik user yang login + ringkasan pengajuan terakhir.</summary>
    public async Task<object?> GetMyStatusAsync(int userId)
    {
        var organizer = await context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
        if (organizer == null) return null;

        var latest = await context.OrganizerVerifications
            .Where(v => v.OrganizerId == organizer.OrganizerId)
            .OrderByDescending(v => v.SubmittedAt)
            .Select(v => new { v.VerificationId, v.Status, v.SubmittedAt, v.ReviewedAt, v.RejectedReason })
            .FirstOrDefaultAsync();

        var portfolioCount = await context.OrganizerPortfolioItems
            .CountAsync(p => p.OrganizerId == organizer.OrganizerId);

        return new
        {
            organizer.OrganizerId,
            organizer.OrganizationName,
            organizer.VerificationStatus,
            organizer.IsVerified,
            portfolioCount,
            canSubmit = organizer.VerificationStatus is "NOT_SUBMITTED" or "REJECTED",
            latest
        };
    }

    /// <summary>Daftar pengajuan untuk Verification Center Admin (Bab 22), difilter status.</summary>
    public async Task<List<object>> GetListAsync(string? status)
    {
        var query = context.OrganizerVerifications
            .Include(v => v.Organizer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (!AllowedStatusFilters.Contains(status, StringComparer.OrdinalIgnoreCase))
                return [];
            query = query.Where(v => v.Status == status);
        }

        return await query
            .OrderBy(v => v.SubmittedAt)
            .Select(v => new
            {
                v.VerificationId,
                organizerId = v.OrganizerId,
                organizerName = v.Organizer!.OrganizationName,
                organizerLocation = v.Organizer.Location,
                v.OrganizationName,
                v.Description,
                v.Location,
                v.WebsiteUrl,
                v.Contact,
                v.PortfolioCount,
                v.PortfolioSummary,
                v.Status,
                v.SubmittedAt
            })
            .ToListAsync<object>();
    }
}
