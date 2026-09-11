using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace GigRadarMobile.Services;

public sealed class LocationService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public bool IsLocationEnabled => Geolocation.Default.IsEnabled;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied)
            return false;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status == PermissionStatus.Granted;
    }

    public async Task<Location?> GetCurrentLocationAsync(
        GeolocationAccuracy accuracy = GeolocationAccuracy.Medium,
        CancellationToken cancellationToken = default)
    {
        var permissionGranted = await RequestPermissionAsync();

        if (!permissionGranted)
            return null;

        if (!Geolocation.Default.IsEnabled)
            return null;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

            timeoutCts.CancelAfter(RequestTimeout);

            var request = new GeolocationRequest(
                accuracy,
                RequestTimeout);

#if IOS
            request.RequestFullAccuracy = false;
#endif

            return await Geolocation.Default.GetLocationAsync(
                request,
                timeoutCts.Token);
        }
        catch (PermissionException)
        {
            return null;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task<Location?> GetLastKnownLocationAsync()
    {
        try
        {
            return await Geolocation.Default.GetLastKnownLocationAsync();
        }
        catch
        {
            return null;
        }
    }
}
