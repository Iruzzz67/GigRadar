namespace GigRadarMobile.Services;

/// <summary>
/// Konfigurasi alamat API terpusat — jangan menulis URL di ViewModel/Halaman.
/// </summary>
public static class ApiConfiguration
{
#if DEBUG && ANDROID
    // Device USB: localhost diteruskan ke PC melalui `adb reverse tcp:5000 tcp:5000`.
    // Jalankan kembali adb reverse setiap device tersambung ulang.
    // Emulator Android: ganti ke http://10.0.2.2:5000/ (10.0.2.2 = loopback host).
    public const string BaseUrl = "http://localhost:5000/";
#elif RELEASE && ANDROID
    // Production API — SnapDeploy container (HTTPS, auto HTTPS cert).
    // Deploy: branch feat/android-production-apk-cli → snapdeploy.dev (free tier, auto-sleep).
    public const string BaseUrl = "https://gigradar-6d752.containers.snapdeploy.app/";
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
