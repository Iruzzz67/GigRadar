using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Gate verifikasi EO di sisi klien (§53/§54): mencegah EO yang belum terverifikasi
/// mengisi form event panjang lalu ditolak server. Server tetap penentu akhir —
/// bila status tidak bisa dimuat (jaringan), EO dibiarkan lanjut dan server yang menolak.
/// </summary>
public static class OrganizerGate
{
    /// <summary>
    /// True bila EO terverifikasi (atau status tidak dapat dipastikan). Bila jelas belum
    /// terverifikasi, tampilkan penjelasan + tawaran membuka halaman verifikasi.
    /// </summary>
    public static async Task<bool> CanManageEventsAsync(ApiService api, AuthService auth)
    {
        OrganizerVerificationStatus? verification;
        try
        {
            api.SetAuthToken(auth.GetToken());
            verification = await api.GetMyOrganizerVerificationStatusAsync();
        }
        catch
        {
            // Status tak terbaca (mis. jaringan) — jangan blokir di klien.
            return true;
        }

        if (verification?.IsVerified == true) return true;

        var openVerification = await Alerts.ConfirmAsync(
            "Verifikasi organizer dulu",
            $"Status verifikasi: {verification?.StatusDisplay ?? "Belum mengajukan"}.\n\n" +
            "Event hanya bisa dibuat dan dikelola setelah Admin menyetujui verifikasi " +
            "(lengkapi profil organisasi + portofolio lebih dulu).\n\n" +
            "Buka halaman verifikasi sekarang?",
            accept: "Buka Verifikasi", cancel: "Nanti");

        if (openVerification)
            await Shell.Current.GoToAsync(nameof(OrganizerVerificationPage));

        return false;
    }
}
