using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Models;
using GigRadarMobile.Helpers;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private ObservableCollection<GigEvent> _recommendedEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _nearbyEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _tonightEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _weekendEvents = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRefreshing;
        [ObservableProperty] private string _userName = "";

        public HomeViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
            UserName = _auth.GetUserName();
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                UserName = _auth.GetUserName();

                var allEvents = await _api.GetEventsAsync();

                // Hanya tampilkan event yang masih berjalan: Published atau SoldOut.
                // Draft & Completed (selesai) tidak muncul di Discover.
                var visibleEvents = allEvents
                    .Where(e => e.Status is "Published" or "SoldOut")
                    .ToList();

                RecommendedEvents = new ObservableCollection<GigEvent>(visibleEvents.Take(5));
                NearbyEvents = new ObservableCollection<GigEvent>(visibleEvents.Take(3));

                TonightEvents = new ObservableCollection<GigEvent>(
                    visibleEvents.Where(e => e.StartDate.Date == DateTime.Today));

                WeekendEvents = new ObservableCollection<GigEvent>(
                    visibleEvents.Where(e => e.StartDate >= DateTime.Today && e.StartDate <= DateTime.Today.AddDays(7)));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", $"Gagal memuat: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task GoToDetailAsync(GigEvent gigEvent)
        {
            if (gigEvent == null) return;
            await Shell.Current.GoToAsync(nameof(EventDetailPage),
                new Dictionary<string, object> { { "Event", gigEvent } });
        }

        [RelayCommand]
        private async Task GoToArtistAsync(Artist artist)
        {
            if (artist == null) return;
            await Shell.Current.GoToAsync(nameof(ArtistDetailPage),
                new Dictionary<string, object> { { "Artist", artist } });
        }
    }
}
