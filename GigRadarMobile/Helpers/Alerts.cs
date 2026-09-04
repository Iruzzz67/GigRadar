namespace GigRadarMobile.Helpers;

/// <summary>
/// Menampilkan alert tanpa crash saat aplikasi belum berada dalam Shell
/// (mis. halaman Login/Onboarding memakai NavigationPage biasa).
/// </summary>
public static class Alerts
{
    public static Task ShowAsync(string title, string message, string cancel = "OK")
    {
        if (Shell.Current is { } shell)
            return shell.DisplayAlertAsync(title, message, cancel);

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page?.DisplayAlertAsync(title, message, cancel) ?? Task.CompletedTask;
    }

    public static Task<bool> ConfirmAsync(string title, string message, string accept = "Ya", string cancel = "Batal")
    {
        if (Shell.Current is { } shell)
            return shell.DisplayAlertAsync(title, message, accept, cancel);

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page?.DisplayAlertAsync(title, message, accept, cancel) ?? Task.FromResult(false);
    }
}
