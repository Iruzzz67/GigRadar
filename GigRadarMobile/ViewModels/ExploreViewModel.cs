using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;
using Microsoft.Maui.Dispatching;

namespace GigRadarMobile.ViewModels
{
    public partial class ExploreViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private List<GigEvent> _allEvents = new();
        private List<Artist> _allArtists = new();
        private List<Venue> _allVenues = new();
        private bool _initialized;

        // Fallback default: Jakarta
        private double _centerLat = -6.2088;
        private double _centerLng = 106.8456;

        private IDispatcherTimer? _debounce;

        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _viewMode = "List"; // List | Grid | Radar
        [ObservableProperty] private bool _isListView = true;
        [ObservableProperty] private bool _isGridView;
        [ObservableProperty] private bool _isRadarView;
        [ObservableProperty] private string _resultCountLabel = "Mulai cari — atau pilih filter di bawah.";
        [ObservableProperty] private bool _hasResults;
        [ObservableProperty] private bool _hasArtists;
        [ObservableProperty] private bool _hasVenues;
        [ObservableProperty] private string _emptyMessage = "";
        [ObservableProperty] private bool _isFiltered;
        [ObservableProperty] private string _selectedGenre = "Semua";
        [ObservableProperty] private string _dateFilter = "Semua";
        [ObservableProperty] private double _radiusKm = 25;
        [ObservableProperty] private ObservableCollection<GenreFilterItem> _genreFilters = new();
        [ObservableProperty] private ObservableCollection<GigEvent> _events = new();
        [ObservableProperty] private ObservableCollection<Artist> _artists = new();
        [ObservableProperty] private ObservableCollection<Venue> _venues = new();
        [ObservableProperty] private RadarDrawable _radar = new();
        [ObservableProperty] private GigEvent? _selectedEvent;
        [ObservableProperty] private bool _hasSelection;
        [ObservableProperty] private string _selectedName = "";
        [ObservableProperty] private string _selectedMeta = "";
        [ObservableProperty] private string _selectedDistance = "";
        [ObservableProperty] private string _selectedPrice = "";
        [ObservableProperty] private string _selectedDay = "";
        [ObservableProperty] private string _selectedMonth = "";

        public ExploreViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
            Radar.MaxRadiusKm = RadiusKm;
        }

        public async Task OnPageAppearingAsync()
        {
            if (!_initialized && !IsLoading)
                await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                _api.SetAuthToken(_auth.GetToken());

                var location = await TryGetLocationAsync();
                if (location != null)
                {
                    _centerLat = location.Latitude;
                    _centerLng = location.Longitude;
                }

                var eventsTask = _api.GetEventsAsync();
                var artistsTask = _api.GetArtistsAsync();
                var venuesTask = _api.GetVenuesAsync();
                var genresTask = _api.GetGenresAsync();

                await Task.WhenAll(eventsTask, artistsTask, venuesTask, genresTask);

                _allEvents = eventsTask.Result;
                _allArtists = artistsTask.Result;
                _allVenues = venuesTask.Result;

                var chips = new ObservableCollection<GenreFilterItem> { new() { Name = "Semua", IsSelected = true } };
                foreach (var g in genresTask.Result) chips.Add(new GenreFilterItem { Name = g.Name });
                GenreFilters = chips;

                _initialized = true;
                ApplyFilters();
            }
            catch (Exception ex)
            {
                EmptyMessage = $"Gagal memuat: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            // Debounce 300 ms — cari as-you-type tanpa spam.
            _debounce?.Stop();
            _debounce = Application.Current?.Dispatcher.CreateTimer();
            if (_debounce == null)
            {
                ApplyFilters();
                return;
            }
            _debounce.Interval = TimeSpan.FromMilliseconds(300);
            _debounce.Tick += (_, _) =>
            {
                _debounce?.Stop();
                ApplyFilters();
            };
            _debounce.Start();
        }

        partial void OnViewModeChanged(string value)
        {
            IsListView = value == "List";
            IsGridView = value == "Grid";
            IsRadarView = value == "Radar";
        }

        [RelayCommand]
        private void SetViewMode(string mode)
        {
            if (mode is "List" or "Grid" or "Radar")
                ViewMode = mode;
        }

        [RelayCommand]
        private void Search(string _) => ApplyFilters();

        [RelayCommand]
        private void SelectGenre(string? genre)
        {
            SelectedGenre = string.IsNullOrWhiteSpace(genre) ? "Semua" : genre;

            foreach (var filter in GenreFilters)
                filter.IsSelected = filter.Name == SelectedGenre;

            ApplyFilters();
        }

        [RelayCommand]
        private void SelectDate(string? date)
        {
            if (date is "Semua" or "Malam ini" or "Akhir pekan")
            {
                DateFilter = date;
                ApplyFilters();
            }
        }

        [RelayCommand]
        private void SelectRadius(string option)
        {
            if (double.TryParse(option.Replace("km", "").Trim(), out var km) && km > 0)
            {
                RadiusKm = km;
                Radar.MaxRadiusKm = km;
                ApplyFilters();
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = "";
            SelectedGenre = "Semua";
            DateFilter = "Semua";
            _debounce?.Stop();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!_initialized) return;

            var query = SearchText?.Trim() ?? "";
            var genre = SelectedGenre == "Semua" ? null : SelectedGenre;

            var filtered = _allEvents.Where(e => e.Status is "Published" or "SoldOut");

            if (!string.IsNullOrEmpty(query))
            {
                var q = query.ToLowerInvariant();
                filtered = filtered.Where(e =>
                    e.Name.ToLowerInvariant().Contains(q) ||
                    e.VenueName.ToLowerInvariant().Contains(q) ||
                    e.GenreName.ToLowerInvariant().Contains(q) ||
                    e.LineupNames.ToLowerInvariant().Contains(q));
            }

            if (genre != null)
            {
                filtered = filtered.Where(e =>
                    string.Equals(e.GenreName, genre, StringComparison.OrdinalIgnoreCase));
            }

            if (DateFilter == "Malam ini")
            {
                filtered = filtered.Where(e => e.StartDate.Date == DateTime.Today);
            }
            else if (DateFilter == "Akhir pekan")
            {
                filtered = filtered.Where(e =>
                    e.StartDate >= DateTime.Today && e.StartDate <= DateTime.Today.AddDays(7));
            }

            // Radius (km) — hitung jarak sekaligus untuk sorting.
            var withDistance = filtered
                .Select(e => new { Event = e, Distance = GeoHelper.HaversineKm(_centerLat, _centerLng, e.Latitude, e.Longitude) })
                .Where(x => x.Distance <= RadiusKm)
                .OrderBy(x => x.Event.StartDate)
                .ToList();

            Events = new ObservableCollection<GigEvent>(withDistance.Select(x => x.Event));

            // Artist + venue hanya dicari saat ada query.
            if (!string.IsNullOrEmpty(query))
            {
                var q = query.ToLowerInvariant();
                Artists = new ObservableCollection<Artist>(
                    _allArtists.Where(a =>
                        a.Name.ToLowerInvariant().Contains(q) ||
                        a.Genre.ToLowerInvariant().Contains(q)).Take(6));
                Venues = new ObservableCollection<Venue>(
                    _allVenues.Where(v =>
                        v.Name.ToLowerInvariant().Contains(q) ||
                        v.City.ToLowerInvariant().Contains(q)).Take(6));
            }
            else
            {
                Artists = new ObservableCollection<Artist>();
                Venues = new ObservableCollection<Venue>();
            }

            HasArtists = Artists.Count > 0;
            HasVenues = Venues.Count > 0;
            IsFiltered = !string.IsNullOrEmpty(query) || genre != null || DateFilter != "Semua";

            HasResults = Events.Count > 0 || HasArtists || HasVenues;

            ResultCountLabel = BuildCountLabel();
            EmptyMessage = !HasResults
                ? (IsFiltered ? "Tidak ada hasil yang cocok. Coba ubah kata kunci atau filter." : "Belum ada gig dalam radius ini.")
                : "";

            // Radar: node dari hasil yang sudah difilter.
            Radar.Nodes = RadarBuilder.Build(Events, _centerLat, _centerLng, RadiusKm);
            Radar.SelectedIndex = null;

            if (SelectedEvent == null || !Events.Any(e => e.EventId == SelectedEvent.EventId))
                SelectedEvent = Events.FirstOrDefault();

            UpdateSelectedPreview();
        }

        private string BuildCountLabel()
        {
            var parts = new List<string>();
            if (Events.Count > 0) parts.Add($"{Events.Count} gig");
            if (HasArtists) parts.Add($"{Artists.Count} artist");
            if (HasVenues) parts.Add($"{Venues.Count} venue");
            return parts.Count > 0 ? string.Join(" · ", parts) : "";
        }

        private void UpdateSelectedPreview()
        {
            HasSelection = SelectedEvent != null;
            if (SelectedEvent == null)
            {
                SelectedName = SelectedMeta = SelectedDistance = SelectedPrice = SelectedDay = SelectedMonth = "";
                return;
            }

            var e = SelectedEvent;
            SelectedName = e.Name;
            SelectedMeta = $"{e.VenueName} • {e.TimeFormatted}";
            SelectedDistance = $"{GeoHelper.FormatKm(GeoHelper.HaversineKm(_centerLat, _centerLng, e.Latitude, e.Longitude))} dari kamu";
            SelectedPrice = e.PriceFormatted;
            SelectedDay = e.StartDate.ToString("dd");
            SelectedMonth = e.StartDate.ToString("MMM").ToUpperInvariant();
        }

        /// <summary>Tap di GraphicsView radar — pilih node terdekat.</summary>
        public void SelectNodeAt(float x, float y)
        {
            var index = Radar.HitTest(x, y);
            Radar.SelectedIndex = index;
            SelectedEvent = index.HasValue ? Radar.Nodes[index.Value].Event : null;
            UpdateSelectedPreview();
        }

        [RelayCommand]
        private async Task GoToDetailAsync(GigEvent gigEvent)
        {
            if (gigEvent == null) return;
            await Shell.Current.GoToAsync(nameof(EventDetailPage),
                new Dictionary<string, object> { { "Event", gigEvent } });
        }

        [RelayCommand]
        private async Task GoToSelectedAsync()
        {
            if (SelectedEvent != null)
                await GoToDetailAsync(SelectedEvent);
        }

        [RelayCommand]
        private async Task GoToArtistAsync(Artist artist)
        {
            if (artist == null) return;
            await Shell.Current.GoToAsync(nameof(ArtistDetailPage),
                new Dictionary<string, object> { { "Artist", artist } });
        }

        [RelayCommand]
        private async Task OpenVenueMapAsync(Venue? venue)
        {
            if (venue == null) return;
            var url = $"https://www.google.com/maps?q={venue.Latitude},{venue.Longitude}";
            await Launcher.OpenAsync(url);
        }

        private static async Task<Location?> TryGetLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

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
                return null;
            }
        }
    }
}