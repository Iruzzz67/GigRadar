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
        private readonly LocationService _locationService;

        // Titik pusat teaser radar di Home — fallback Jakarta, konsisten dengan halaman Radar.
        private const double TeaserLat = -6.2088;
        private const double TeaserLng = 106.8456;

        [ObservableProperty] private ObservableCollection<GigEvent> _recommendedEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _nearbyEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _tonightEvents = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _weekendEvents = new();
        [ObservableProperty] private ObservableCollection<Artist> _artists = new();
        [ObservableProperty] private ObservableCollection<string> _genreChips = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRefreshing;
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _cityLabel = "Jakarta";
        [ObservableProperty] private string _initials = "";
        [ObservableProperty] private string _greetingEyebrow = "MALAM INI DI JAKARTA";
        [ObservableProperty] private GigEvent? _featuredEvent;
        [ObservableProperty] private bool _hasFeatured;
        [ObservableProperty] private RadarDrawable _teaserRadar = new();
        [ObservableProperty] private bool _hasWeekendEvents;
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private string _errorMessage = "";

        public enum LocationStatusType
        {
            Unknown,
            RequestingPermission,
            PermissionDenied,
            Disabled,
            Available,
            Unavailable,
            Error
        }

        [ObservableProperty] private LocationStatusType _locationStatus = LocationStatusType.Unknown;
        [ObservableProperty] private double? _currentLatitude;
        [ObservableProperty] private double? _currentLongitude;

        public bool IsUsingDeviceLocation => CurrentLatitude.HasValue && CurrentLongitude.HasValue;

        public HomeViewModel(ApiService api, AuthService auth, LocationService locationService)
        {
            _api = api;
            _auth = auth;
            _locationService = locationService;
            UserName = _auth.GetUserName();
            CityLabel = Preferences.Default.Get("user_city", "Jakarta");
            GreetingEyebrow = $"MALAM INI DI {CityLabel.ToUpperInvariant()}";
            Initials = BuildInitials(UserName);
            TeaserRadar.ReducedMotion = true; // teaser statis — hemat daya
            TeaserRadar.MaxRadiusKm = 50;
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                UserName = _auth.GetUserName();
                Initials = BuildInitials(UserName);

                var allEvents = await _api.GetEventsAsync();

                // Hanya event yang masih berjalan: Published atau SoldOut.
                var visibleEvents = allEvents
                    .Where(e => e.Status is "Published" or "SoldOut")
                    .ToList();

                FeaturedEvent = visibleEvents
                    .FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.PosterUrl))
                    ?? visibleEvents.FirstOrDefault();
                HasFeatured = FeaturedEvent != null;

                RecommendedEvents = new ObservableCollection<GigEvent>(visibleEvents.Take(5));

                // Fetch Nearby Events based on device location
                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    CurrentLatitude = location.Latitude;
                    CurrentLongitude = location.Longitude;
                    LocationStatus = LocationStatusType.Available;
                    
                    var nearbyFromApi = await _api.GetNearbyEventsAsync(location.Latitude, location.Longitude, 50);
                    NearbyEvents = new ObservableCollection<GigEvent>(
                        nearbyFromApi.Where(e => e.Status is "Published" or "SoldOut").Take(4));
                }
                else
                {
                    LocationStatus = LocationStatusType.Unavailable;
                    NearbyEvents = new ObservableCollection<GigEvent>(visibleEvents.Take(4));
                }

                TonightEvents = new ObservableCollection<GigEvent>(
                    visibleEvents.Where(e => e.StartDate.Date == DateTime.Today));

                var weekend = visibleEvents
                    .Where(e => e.StartDate >= DateTime.Today && e.StartDate <= DateTime.Today.AddDays(7))
                    .OrderBy(e => e.StartDate)
                    .ToList();
                WeekendEvents = new ObservableCollection<GigEvent>(weekend);
                HasWeekendEvents = weekend.Count > 0;

                // Teaser radar: node nyata dari event terdekat (pusat: Jakarta).
                var teaserNodes = RadarBuilder.Build(visibleEvents, TeaserLat, TeaserLng, 50, limit: 6);
                TeaserRadar.Nodes = teaserNodes;

                // Artist + genre chips (gagal diam-diam — bukan konten utama).
                try
                {
                    var artistsTask = _api.GetArtistsAsync();
                    var genresTask = _api.GetGenresAsync();
                    await Task.WhenAll(artistsTask, genresTask);

                    Artists = new ObservableCollection<Artist>(artistsTask.Result.Take(6));

                    var chips = new ObservableCollection<string> { "Semua" };
                    foreach (var g in genresTask.Result) chips.Add(g.Name);
                    GenreChips = chips;
                }
                catch { /* non-kritis */ }
            }
            catch (Exception ex)
            {
                // Error tampil inline dengan tombol coba lagi — bukan empty state yang menyesatkan.
                HasError = true;
                ErrorMessage = "Gagal memuat: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        /// <summary>Ubah kota via dialog — tersimpan ke profil (PUT /api/users/me) + preference lokal.</summary>
        [RelayCommand]
        private async Task ChangeCityAsync()
        {
            var newCity = await Alerts.PromptAsync("Ubah kota",
                "Kota untuk rekomendasi dan radar kamu:",
                placeholder: "Nama kota", initialValue: CityLabel);

            if (string.IsNullOrWhiteSpace(newCity)) return;
            var trimmed = newCity.Trim();
            if (string.Equals(trimmed, CityLabel, StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                // name & photoUrl null → nilai lama dipertahankan server.
                var updated = await _api.UpdateProfileAsync(name: null, city: trimmed);
                if (updated == null)
                {
                    await Alerts.ShowAsync("Gagal", "Kota tidak bisa disimpan. Periksa koneksi lalu coba lagi.");
                    return;
                }

                Preferences.Default.Set("user_city", trimmed);
                CityLabel = trimmed;
                GreetingEyebrow = $"MALAM INI DI {trimmed.ToUpperInvariant()}";
                await LoadEventsCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>Avatar di header → tab Profile.</summary>
        [RelayCommand]
        private async Task GoToProfileAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//profile");
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
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

        /// <summary>Chip genre di Home → buka tab Radar dengan genre itu terpilih.</summary>
        [RelayCommand]
        private async Task GoToGenreRadarAsync(string? genre)
        {
            RadarState.SelectedGenre = string.IsNullOrWhiteSpace(genre) || genre == "Semua" ? null : genre;
            await Shell.Current.GoToAsync("//radar");
        }

        [RelayCommand]
        private async Task GoToRadarAsync()
        {
            await Shell.Current.GoToAsync("//radar");
        }

        private static string BuildInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }
}