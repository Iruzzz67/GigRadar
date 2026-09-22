using Microsoft.Maui.Controls;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Mengganti halaman root aplikasi dengan aman.
/// .NET 10 menandai Application.MainPage sebagai obsolete, jadi dipakai
/// Application.Windows[0].Page bila window sudah ada.
/// </summary>
public static class NavigationHelper
{
    public static void SetRoot(Page page)
    {
        var app = Application.Current;
        if (app is null) return;

        if (app.Windows.Count > 0)
        {
            // Unfocus dulu input yang masih fokus sebelum root diganti.
            // Tanpa ini, clearFocus pada platform view lama terjadi SETELAH
            // handler halaman lama di-dispose -> InputView.MapIsFocused memanggil
            // GetService pada IServiceProvider yang sudah di-dispose
            // -> ObjectDisposedException (crash saat login/register dari HP).
            DismissFocus(app.Windows[0].Page);

            app.Windows[0].Page = page;
            return;
        }

        // Fallback sebelum window dibuat (tidak terjadi dalam alur normal aplikasi ini).
#pragma warning disable CS0618
        app.MainPage = page;
#pragma warning restore CS0618
    }

    /// <summary>
    /// Melepas fokus Entry/Editor/SearchBar yang sedang fokus (menutup keyboard)
    /// SELAGI handler halaman lama masih hidup, sehingga perubahan fokus diproses
    /// dengan IServiceProvider yang valid.
    /// </summary>
    private static void DismissFocus(Page? root)
    {
        if (root is null) return;

        try
        {
            var focused = root.GetVisualTreeDescendants()
                .OfType<InputView>() // Entry, Editor, SearchBar
                .FirstOrDefault(v => v.IsFocused);

            focused?.Unfocus();
        }
        catch
        {
            // Keyboard sudah tertutup / handler tidak tersedia — aman diabaikan.
        }
    }
}
