namespace GigRadarMobile.Helpers;

/// <summary>Deteksi preferensi "kurangi gerakan" (reduced motion) dari OS.</summary>
public static class ReducedMotion
{
    public static bool IsEnabled
    {
        get
        {
#if ANDROID
            try
            {
                var resolver = Microsoft.Maui.ApplicationModel.Platform.AppContext?.ContentResolver;
                if (resolver == null) return false;
                var scale = Android.Provider.Settings.Global.GetFloat(
                    resolver,
                    Android.Provider.Settings.Global.AnimatorDurationScale,
                    1f);
                return scale == 0f;
            }
            catch { return false; }
#elif IOS || MACCATALYST
            try { return UIKit.UIAccessibility.IsReduceMotionEnabled; }
            catch { return false; }
#elif WINDOWS
            try
            {
                var settings = new Windows.UI.ViewManagement.UISettings();
                return !settings.AnimationsEnabled;
            }
            catch { return false; }
#else
            return false;
#endif
        }
    }
}