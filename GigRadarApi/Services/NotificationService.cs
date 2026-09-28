using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Services;

/// <summary>
/// Membuat notifikasi untuk user (GIGRADAR_APP_SPEC.md Bab 36).
/// Dipanggil dari alur lain: verifikasi (ARTIST_VERIFICATION/EO_VERIFICATION),
/// undangan (ARTIST_INVITATION), pembelian tiket (TICKET_PURCHASE), dst.
/// Tipe mengikuti daftar resmi di Bab 36.
/// </summary>
public class NotificationService(AppDbContext context)
{
    public static readonly string[] AllowedTypes =
    [
        "NEW_GIG_NEARBY", "FOLLOWED_ARTIST_NEW_GIG", "ARTIST_NEW_RELEASE", "ARTIST_NEW_POST",
        "EVENT_REMINDER", "TICKET_PURCHASE", "TICKET_UPDATE", "EVENT_CANCELLED",
        "ARTIST_INVITATION", "GIG_APPLICATION", "GIG_APPLICATION_UPDATE",
        "ARTIST_VERIFICATION", "EO_VERIFICATION", "ADMIN_NOTICE"
    ];

    /// <summary>Buat satu notifikasi. Type di luar daftar diganti ADMIN_NOTICE.</summary>
    public async Task NotifyAsync(int userId, string type, string title, string body, string link = "")
    {
        var safeType = AllowedTypes.Contains(type) ? type : "ADMIN_NOTICE";
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = safeType,
            Title = title,
            Body = body,
            Link = link
        });
        await context.SaveChangesAsync();
    }

    /// <summary>Daftar notifikasi user, terbaru dulu; hanya yang belum dibaca bila unreadOnly.</summary>
    public async Task<List<object>> GetMyAsync(int userId, bool unreadOnly)
    {
        var query = context.Notifications.Where(n => n.UserId == userId).AsQueryable();
        if (unreadOnly) query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .Select(n => new
            {
                n.NotificationId,
                n.Type,
                n.Title,
                n.Body,
                n.Link,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync<object>();
    }

    /// <summary>Tandai satu notifikasi dibaca (milik sendiri).</summary>
    public async Task<(bool Success, string Message)> MarkReadAsync(int userId, int notificationId)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
        if (notification == null) return (false, "Notifikasi tidak ditemukan");

        notification.IsRead = true;
        await context.SaveChangesAsync();
        return (true, "Notifikasi ditandai dibaca");
    }

    /// <summary>Tandai semua notifikasi user dibaca.</summary>
    public async Task<int> MarkAllReadAsync(int userId)
    {
        var unread = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await context.SaveChangesAsync();
        return unread.Count;
    }

    /// <summary>Jumlah notifikasi belum dibaca (badge).</summary>
    public Task<int> GetUnreadCountAsync(int userId) =>
        context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
}
