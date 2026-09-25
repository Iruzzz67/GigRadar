using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace GigRadarApi.Authorization;

/// <summary>
/// Mengembalikan 403 dari endpoint API sebagai JSON { message, code }.
/// Tanpa ini respons 403 body-nya kosong dan klien mobile (yang selalu
/// mendeserialisasi body) akan gagal karena bukan JSON.
/// </summary>
public sealed class ApiAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context,
        AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        // Hanya tangani 403 dari gate verifikasi organizer; 401 dan 403 lain
        // tetap memakai perilaku bawaan ASP.NET Core.
        var code = authorizeResult.Forbidden
            ? authorizeResult.AuthorizationFailure?.FailureReasons
                .Select(reason => reason.Message)
                .FirstOrDefault(message => message is VerifiedOrganizerHandler.OrganizerNotVerifiedCode
                    or VerifiedOrganizerHandler.NotOrganizerCode)
            : null;

        if (code != null && context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                message = code == VerifiedOrganizerHandler.OrganizerNotVerifiedCode
                    ? "Akun EO kamu belum terverifikasi, jadi belum bisa membuat event atau mengelola tiket. "
                      + "Lengkapi profil organisasi + portofolio lalu ajukan verifikasi; event bisa berjalan setelah Admin menyetujuinya."
                    : "Hanya akun EO terverifikasi yang dapat melakukan aksi ini.",
                code
            });
            return;
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
