using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Services;

/// <summary>Body POST /api/artist/verification (Bab 7 — Artist Application).</summary>
public class ArtistVerificationRequest
{
    public string? LegalName { get; set; }
    public string? Phone { get; set; }
    public string? SpotifyUrl { get; set; }
    public string? SupportingInfo { get; set; }
}

/// <summary>
/// Logika bisnis verifikasi artist (GIGRADAR_APP_SPEC.md Bab 7–8, 22):
/// - Artist submit pengajuan verifikasi (data pendukung wajib).
/// - Hanya Admin yang dapat approve/reject — status di Artist selalu dihitung server.
/// - Setiap keputusan tercatat (ReviewedBy/RejectedReason/ReviewedAt) sebagai jejak audit.
/// - Pengajuan yang ditolak boleh diajukan ulang (PENDING baru diperbolehkan setelah REJECTED).
/// </summary>
public class ArtistVerificationService(AppDbContext context)
{
    /// <summary>Filter status yang valid untuk query daftar pengajuan.</summary>
    public static readonly string[] AllowedStatusFilters = ["PENDING", "VERIFIED", "REJECTED"];

    public async Task<(bool Success, string Message, object? Result)> SubmitAsync(
        int userId, ArtistVerificationRequest request)
    {
        var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
        if (artist == null) return (false, "Profil artist belum ada", null);

        // Validasi: nama legal, telepon, dan minimal satu bukti pendukung wajib (Bab 7).
        if (string.IsNullOrWhiteSpace(request.LegalName))
            return (false, "Nama legal / perwakilan wajib diisi", null);

        if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Trim().Length < 9)
            return (false, "Nomor telepon tidak valid (minimal 9 digit)", null);

        if (string.IsNullOrWhiteSpace(request.SpotifyUrl)
            && string.IsNullOrWhiteSpace(request.SupportingInfo))
            return (false, "Sertakan link Spotify atau informasi pendukung sebagai bukti", null);

        // Satu pengajuan PENDING per artist (Bab 8 — pengajuan ditolak boleh diajukan ulang).
        var hasPending = await context.ArtistVerifications
            .AnyAsync(v => v.ArtistId == artist.ArtistId && v.Status == "PENDING");
        if (hasPending)
            return (false, "Kamu sudah punya pengajuan yang sedang ditinjau", null);

        var verification = new ArtistVerification
        {
            ArtistId = artist.ArtistId,
            LegalName = request.LegalName.Trim(),
            Phone = request.Phone.Trim(),
            SpotifyUrl = request.SpotifyUrl?.Trim() ?? "",
            SupportingInfo = request.SupportingInfo?.Trim() ?? "",
            Status = "PENDING",
            SubmittedAt = DateTime.UtcNow
        };

        context.ArtistVerifications.Add(verification);
        artist.VerificationStatus = "PENDING";
        artist.IsVerified = false;

        await context.SaveChangesAsync();
        return (true, "Pengajuan verifikasi terkirim — menunggu tinjauan Admin", verification);
    }

    public async Task<(bool Success, string Message)> ReviewAsync(
        int verificationId, int adminUserId, bool approve, string? reason)
    {
        var verification = await context.ArtistVerifications
            .Include(v => v.Artist)
            .FirstOrDefaultAsync(v => v.VerificationId == verificationId);

        if (verification == null)
            return (false, "Pengajuan verifikasi tidak ditemukan");

        if (verification.Status != "PENDING")
            return (false, "Pengajuan ini sudah diputuskan");

        // Pastikan artist masih ada (penghapusan artist seharusnya menghapus pengajuan).
        if (verification.Artist == null)
            return (false, "Artist pemilik pengajuan tidak ditemukan");

        verification.Status = approve ? "VERIFIED" : "REJECTED";
        verification.ReviewedAt = DateTime.UtcNow;
        verification.ReviewedByUserId = adminUserId;

        if (!approve)
        {
            verification.RejectedReason = string.IsNullOrWhiteSpace(reason)
                ? "Tidak memenuhi kriteria verifikasi"
                : reason.Trim();
        }

        // Status profil mengikuti keputusan terakhir (server-controlled, Bab 8).
        verification.Artist.VerificationStatus = verification.Status;
        verification.Artist.IsVerified = approve;

        await context.SaveChangesAsync();
        var msg = approve
            ? $"\"{verification.Artist.Name}\" terverifikasi"
            : $"Pengajuan \"{verification.Artist.Name}\" ditolak";
        return (true, msg);
    }

    /// <summary>Status verifikasi artist milik user yang login.</summary>
    public async Task<object?> GetMyStatusAsync(int userId)
    {
        var artist = await context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
        if (artist == null) return null;

        var latest = await context.ArtistVerifications
            .Where(v => v.ArtistId == artist.ArtistId)
            .OrderByDescending(v => v.SubmittedAt)
            .Select(v => new
            {
                v.VerificationId,
                v.Status,
                v.SubmittedAt,
                v.ReviewedAt,
                v.RejectedReason
            })
            .FirstOrDefaultAsync();

        return new
        {
            artist.ArtistId,
            artist.Name,
            artist.VerificationStatus,
            artist.IsVerified,
            canSubmit = artist.VerificationStatus is "NOT_SUBMITTED" or "REJECTED",
            latest
        };
    }

    /// <summary>Daftar pengajuan untuk Verification Center Admin (Bab 22), difilter status.</summary>
    public async Task<List<object>> GetListAsync(string? status)
    {
        var query = context.ArtistVerifications
            .Include(v => v.Artist)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            // Whitelist agar query tidak error di kolom enum/teks tak dikenal.
            if (!AllowedStatusFilters.Contains(status, StringComparer.OrdinalIgnoreCase))
                return [];
            query = query.Where(v => v.Status == status);
        }

        return await query
            .OrderBy(v => v.SubmittedAt)
            .Select(v => new
            {
                v.VerificationId,
                artistId = v.ArtistId,
                artistName = v.Artist!.Name,
                artistGenre = v.Artist.Genre,
                artistCity = v.Artist.City,
                v.LegalName,
                v.Phone,
                v.SpotifyUrl,
                v.SupportingInfo,
                v.Status,
                v.SubmittedAt
            })
            .ToListAsync<object>();
    }
}
