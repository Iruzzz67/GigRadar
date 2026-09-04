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
            app.Windows[0].Page = page;
            return;
        }

        // Fallback sebelum window dibuat (tidak terjadi dalam alur normal aplikasi ini).
#pragma warning disable CS0618
        app.MainPage = page;
#pragma warning restore CS0618
    }
}
