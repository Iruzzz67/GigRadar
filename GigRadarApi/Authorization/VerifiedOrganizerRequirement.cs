using GigRadarApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Authorization;

/// <summary>
/// Policy "VerifiedOrganizer" (GIGRADAR_APP_SPEC.md §53/§54):
/// membuat/mengubah event dan mengelola tiket butuh Authenticated + Role=EO +
/// organizer terverifikasi Admin. Ini gate anti-penipuan: EO yang belum lolos
/// verifikasi tidak boleh menjual tiket atau menayangkan event.
/// </summary>
public sealed class VerifiedOrganizerRequirement : IAuthorizationRequirement;

/// <summary>
/// Memeriksa status verifikasi organizer langsung dari database (server tetap
/// satu-satunya sumber kebenaran — klien tidak boleh menentukan status verifikasi).
/// </summary>
public sealed class VerifiedOrganizerHandler(AppDbContext db)
    : AuthorizationHandler<VerifiedOrganizerRequirement>
{
    /// <summary>Alasan 403 saat EO belum terverifikasi (dipakai klien untuk pesan yang bisa ditindaklanjuti).</summary>
    public const string OrganizerNotVerifiedCode = "ORGANIZER_NOT_VERIFIED";

    /// <summary>Alasan 403 saat pemanggil bukan EO (fallback, tetap 403 biasa).</summary>
    public const string NotOrganizerCode = "ROLE_NOT_EO";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, VerifiedOrganizerRequirement requirement)
    {
        // Admin mengelola platform (bukan menyelenggarakan event sebagai EO) dan
        // tidak punya profil organizer — Admin tidak diblokir gate ini.
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Selain EO/Admin tetap ditolak lewat pemeriksaan role biasa.
        if (!context.User.IsInRole("EO"))
        {
            context.Fail(new AuthorizationFailureReason(this, NotOrganizerCode));
            return;
        }

        if (!int.TryParse(context.User.FindFirst("UserId")?.Value, out var userId))
        {
            context.Fail(new AuthorizationFailureReason(this, OrganizerNotVerifiedCode));
            return;
        }

        // Profil organizer tanpa baris di database dianggap belum terverifikasi,
        // sehingga EO wajib melengkapi profil + portofolio dan lolos review Admin dulu.
        var isVerified = await db.Organizers
            .AnyAsync(o => o.UserId == userId && o.IsVerified);

        if (isVerified)
            context.Succeed(requirement);
        else
            context.Fail(new AuthorizationFailureReason(this, OrganizerNotVerifiedCode));
    }
}
