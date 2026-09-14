using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    /// <summary>Chip filter genre — IsSelected dipakai DataTrigger di XAML.</summary>
    public partial class GenreFilterItem : ObservableObject
    {
        public required string Name { get; init; }
        [ObservableProperty] private bool _isSelected;
    }

    public partial class RadarViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly LocationService _locationService;
        private List<GigEvent> _allEvents = new();

        [ObservableProperty] private double _centerLat = -6.2088;
        [ObservableProperty] private double _centerLng = 106.8456;

        [ObservableProperty] private ObservableCollection<GigEvent> _events = new();
        [ObservableProperty] private ObservableCollection<GenreFilterItem> _genreFilters = new();
        [ObservableProperty] private RadarDrawable _radar = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRefreshing;
        [ObservableProperty] private string _cityLabel = "Jakarta";
        [ObservableProperty] private string _countLabel = "";
        [ObservableProperty] private double _radiusKm = 25;
        [ObservableProperty] private bool _tonightOnly;
        [ObservableProperty] private bool _hasEvents;
        [ObservableProperty] private bool _isFilterActive;
        [ObservableProperty] private string _emptyMessage = "Cari gig di sekitar lokasimu.";
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private string _errorMessage = "";
        [ObservableProperty] private GigEvent? _selectedEvent;
        [ObservableProperty] private bool _hasSelection;
        [ObservableProperty] private string _selectedName = "";
        [ObservableProperty] private string _selectedMeta = "";
        [ObservableProperty] private string _selectedDistance = "";
        [ObservableProperty] private string _selectedPrice = "";
        [ObservableProperty] private string _selectedDay = "";
        [ObservableProperty] private string _selectedMonth = "";

        public RadarViewModel(ApiService api, AuthService auth, LocationService locationService)
        {
            _api = api;
            _auth = auth;
            _locationService = locationService;
            Radar.MaxRadiusKm = RadiusKm;
            CityLabel = Preferences.Default.Get("user_city", "Jakarta");
            LoadGenreFilters();
        }

        public void OnPageAppearing()
        {
            // Genre yang dipilih dari chip di Home diterapkan di sini (sekali).
            if (RadarState.SelectedGenre != null)
            {
                SelectGenre(RadarState.SelectedGenre);
                RadarState.SelectedGenre = null;
            }

            if (Events.Count == 0 && !IsLoading)
                LoadCommand.Execute(null);
        }

        private async void LoadGenreFilters()
        {
            try
            {
                var genres = await _api.GetGenresAsync();
                var items = new ObservableCollection<GenreFilterItem>
                {
                    new() { Name = "Semua", IsSelected = true }
                };
                foreach (var g in genres)
                    items.Add(new GenreFilterItem { Name = g.Name });

                GenreFilters = items;
            }
            catch
            {
                GenreFilters = new ObservableCollection<GenreFilterItem> { new() { Name = "Semua", IsSelected = true } };
            }
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            try
            {
                var location = await TryGetLocationAsync();
                if (location != null)
                {
                    CenterLat = location.Latitude;
                    CenterLng = location.Longitude;
                }

                _api.SetAuthToken(_auth.GetToken());
                _allEvents = await _api.GetNearbyEventsAsync(CenterLat, CenterLng, RadiusKm);

                ApplyFilters();
            }
            catch (Exception ex)
            {
                // Error inline dengan retry — bukan popup yang hilang dan bukan empty state palsu.
                HasError = true;
                ErrorMessage = "Gagal memuat radar: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void SelectGenre(string? name)
        {
            var selected = string.IsNullOrWhiteSpace(name) || name == "Semua" ? null : name;

            foreach (var filter in GenreFilters)
                filter.IsSelected = filter.Name == (selected ?? "Semua");

            ApplyFilters();
        }

        [RelayCommand]
        private async Task SelectRadiusAsync(string option)
        {
            if (double.TryParse(option.Replace("km", "").Trim(), out var km) && km > 0)
            {
                RadiusKm = km;
                Radar.MaxRadiusKm = km;
                await LoadAsync();
            }
        }

        [RelayCommand]
        private void ToggleTonight()
        {
            TonightOnly = !TonightOnly;
            ApplyFilters();
        }

        [RelayCommand]
        private void ClearFilter()
        {
            foreach (var filter in GenreFilters)
                filter.IsSelected = filter.Name == "Semua";
            TonightOnly = false;
            ApplyFilters();
        }

        /// <summary>Dari tap di GraphicsView — pilih node terdekat lalu tampilkan preview.</summary>
        public void SelectNodeAt(float x, float y)
        {
            var index = Radar.HitTest(x, y);
            Radar.SelectedIndex = index;
            SelectedEvent = index.HasValue ? Radar.Nodes[index.Value].Event : null;
            UpdateSelectedPreview();
        }

        public void SelectEventById(int eventId)
        {
            var eventItem = Events.FirstOrDefault(item => item.EventId == eventId);
            if (eventItem == null) return;

            SelectedEvent = eventItem;
            UpdateSelectedPreview();
        }

        private void ApplyFilters()
        {
            var selectedGenre = GenreFilters.FirstOrDefault(f => f.IsSelected && f.Name != "Semua")?.Name;

            var nodes = RadarBuilder.Build(
                _allEvents, CenterLat, CenterLng, RadiusKm,
                genre: selectedGenre, tonightOnly: TonightOnly);

            Radar.Nodes = nodes;
            Radar.SelectedIndex = null;

            IsFilterActive = !string.IsNullOrEmpty(selectedGenre) || TonightOnly;
            HasEvents = nodes.Count > 0;

            CountLabel = nodes.Count == 0
                ? "Belum ada gig"
                : $"{nodes.Count} gig di sekitarmu";

            EmptyMessage = nodes.Count == 0
                ? (IsFilterActive
                    ? "Tidak ada gig yang cocok dengan filter ini."
                    : $"Belum ada gig dalam {RadiusKm:0} km dari sini.")
                : "";

            // List terdekat = node terurut jarak.
            Events = new ObservableCollection<GigEvent>(nodes.Select(n => n.Event));

            if (SelectedEvent == null || !nodes.Any(n => n.Event.EventId == SelectedEvent.EventId))
            {
                SelectedEvent = nodes.FirstOrDefault()?.Event;
            }
            UpdateSelectedPreview();
        }

        private void UpdateSelectedPreview()
        {
            HasSelection = SelectedEvent != null;
            if (SelectedEvent == null)
            {
                SelectedName = SelectedMeta = SelectedDistance = SelectedPrice = "";
                return;
            }

            var e = SelectedEvent;
            SelectedName = e.Name;
            SelectedMeta = $"{e.VenueName} • {e.TimeFormatted}";
            SelectedDistance = $"{GeoHelper.FormatKm(GeoHelper.HaversineKm(CenterLat, CenterLng, e.Latitude, e.Longitude))} dari kamu";
            SelectedPrice = e.PriceFormatted;
            SelectedDay = e.StartDate.ToString("dd");
            SelectedMonth = e.StartDate.ToString("MMM").ToUpperInvariant();
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
        private async Task OpenExternalMapAsync(GigEvent? gigEvent)
        {
            if (gigEvent == null) return;
            var url = $"https://www.google.com/maps?q={gigEvent.Latitude},{gigEvent.Longitude}";
            await Launcher.OpenAsync(url);
        }

        private async Task<Location?> TryGetLocationAsync()
        {
            return await _locationService.GetBestAvailableLocationAsync();
        }
    }
}
