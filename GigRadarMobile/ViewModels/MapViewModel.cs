using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly ApiService _api;

        [ObservableProperty] private ObservableCollection<GigEvent> _events = new();
        [ObservableProperty] private bool _isLoading;

        public MapViewModel(ApiService api)
        {
            _api = api;
        }

        [RelayCommand]
        private async Task LoadNearbyEventsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // Fallback default: Jakarta
                double lat = -6.2088;
                double lng = 106.8456;

                var location = await TryGetLocationAsync();
                if (location != null)
                {
                    lat = location.Latitude;
                    lng = location.Longitude;
                }

                var events = await _api.GetNearbyEventsAsync(lat, lng, 50);
                Events = new ObservableCollection<GigEvent>(events);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static async Task<Location?> TryGetLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                    return null;

                return await Geolocation.GetLastKnownLocationAsync()
                       ?? await Geolocation.GetLocationAsync(new GeolocationRequest
                       {
                           DesiredAccuracy = GeolocationAccuracy.Medium,
                           Timeout = TimeSpan.FromSeconds(10)
                       });
            }
            catch
            {
                // Lokasi tidak tersedia/ditolak — halaman tetap aman dengan koordinat default
                return null;
            }
        }

        [RelayCommand]
        private async Task OpenExternalMapAsync(GigEvent? gigEvent)
        {
            if (gigEvent == null) return;
            var url = $"https://www.google.com/maps?q={gigEvent.Latitude},{gigEvent.Longitude}";
            await Launcher.OpenAsync(url);
        }

        [RelayCommand]
        private async Task GoToDetailAsync(GigEvent gigEvent)
        {
            if (gigEvent == null) return;
            await Shell.Current.GoToAsync(nameof(EventDetailPage),
                new Dictionary<string, object> { { "Event", gigEvent } });
        }
    }
}
