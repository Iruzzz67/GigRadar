namespace GigRadarMobile.Services;

/// <summary>
/// Konfigurasi alamat API terpusat — jangan menulis URL di ViewModel/Halaman.
/// </summary>
public static class ApiConfiguration
{
#if ANDROID
    // Device USB: localhost diteruskan ke PC melalui `adb reverse tcp:5000 tcp:5000`.
    // Jalankan kembali adb reverse setiap device tersambung ulang.
    public const string BaseUrl = "http://localhost:5000/";
#elif WINDOWS
    // Windows app berjalan di PC yang sama dengan API
    public const string BaseUrl = "http://localhost:5000/";
#elif IOS
    // Simulator iOS dapat memakai localhost.
    // Device fisik harus menggunakan IP PC.
    public const string BaseUrl = "http://localhost:5000/";
#elif MACCATALYST
    public const string BaseUrl = "http://localhost:5000/";
#else
    public const string BaseUrl = "http://localhost:5000/";
#endif

    /// <summary>
    /// Untuk device fisik (Android/iPhone) yang terhubung ke Wi-Fi yang sama,
    /// ganti dengan IP komputer yang menjalankan API, lalu pakai nilai ini
    /// sebagai BaseUrl. Contoh: http://192.168.68.122:5000
    /// </summary>
    public const string PhysicalDeviceBaseUrl = "http://192.168.68.122:5000";
}
