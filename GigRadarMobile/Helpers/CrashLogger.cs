namespace GigRadarMobile.Helpers;

/// <summary>
/// Mencatat exception yang tidak tertangani ke file log lokal
/// (AppData/gigradar-crash.log) supaya crash yang mematikan proses —
/// khususnya stowed exception XAML di Windows yang tampak hanya
/// sebagai force close — tetap bisa didiagnosis setelah kejadian.
/// </summary>
public static class CrashLogger
{
    private static readonly object Gate = new();

    private static string LogPath =>
        Path.Combine(FileSystem.AppDataDirectory, "gigradar-crash.log");

    public static void Attach()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Write("AppDomain.UnhandledException", e.ExceptionObject as Exception);

        TaskScheduler.UnobservedTaskException += (_, e) =>
            Write("TaskScheduler.UnobservedTaskException", e.Exception);

#if WINDOWS
        if (Microsoft.UI.Xaml.Application.Current is { } app)
            app.UnhandledException += (_, e) =>
            {
                Write("WinUI.Application.UnhandledException", e.Exception, e.Message);

                // Hardening anti force-close: XamlParseException & kawan-kawan
                // saat membuka halaman tidak boleh mematikan proses mentah-mentah.
                // Ditandai Handled agar app tetap hidup; user tinggal dialihkan
                // dari halaman yang bermasalah.
                e.Handled = true;
            };
#endif
    }

    private static void Write(string source, Exception? ex, string? message = null)
    {
        try
        {
            lock (Gate)
            {
                var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}\n{message}\n{ex}\n\n";
                File.AppendAllText(LogPath, text);
            }
        }
        catch
        {
            // Crash logger tidak boleh pernah melempar.
        }
    }
}
