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

    /// <summary>Dialog input satu baris (mis. ubah kota). Mengembalikan null bila dibatalkan/tak tersedia.</summary>
    public static Task<string?> PromptAsync(string title, string message, string placeholder = "",
        string initialValue = "", string accept = "Simpan", string cancel = "Batal")
    {
        if (Shell.Current is { } shell)
            return shell.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength: 60, initialValue: initialValue);

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page?.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength: 60, initialValue: initialValue)
            ?? Task.FromResult<string?>(null);
    }
}
