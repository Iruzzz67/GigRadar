namespace GigRadarMobile.Services;

/// <summary>
/// Centralized API endpoint configuration.
/// DEBUG Android uses the Android emulator host bridge; Release requires the
/// real HTTPS production API endpoint.
/// </summary>
public static class ApiConfiguration
{
#if DEBUG && ANDROID
    public const string BaseUrl = "http://10.0.2.2:5000";
#elif DEBUG && WINDOWS
    public const string BaseUrl = "http://localhost:5000";
#elif DEBUG && IOS
    public const string BaseUrl = "http://localhost:5000";
#elif DEBUG && MACCATALYST
    public const string BaseUrl = "http://localhost:5000";
#else
    // IMPORTANT: replace this value with the deployed HTTPS GigRadar API URL
    // before creating the production Release APK.
    public const string BaseUrl = "https://YOUR-PRODUCTION-API-DOMAIN";
#endif
}
