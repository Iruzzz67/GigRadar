using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Halaman EO/Admin: membuat event baru yang diadakan oleh EO yang memakai
    /// aplikasi ini. Mencakup pemilihan venue (dari daftar atau tambah venue baru),
    /// genre, jadwal, harga, kapasitas, link pembelian eksternal, dan koordinat.
    /// </summary>
    public partial class CreateEventViewModel : ObservableObject
    {
        private const double DefaultLatitude = -6.2088;   // Jakarta (default bila venue tanpa koordinat)
        private const double DefaultLongitude = 106.8456;

        private readonly ApiService _api;
        private readonly AuthService _auth;

        private List<Venue> _venues = new();
        private List<Genre> _genres = new();
        private Venue? _selectedVenue;

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string _statusMessage = "";

        // Info event
        [ObservableProperty] private string _eventName = "";
        [ObservableProperty] private string _eventDescription = "";
        [ObservableProperty] private string _posterUrl = "";
        [ObservableProperty] private string _ticketLink = "";

        // Genre (ditampilkan lewat daftar nama agar Picker sederhana)
        [ObservableProperty] private ObservableCollection<string> _genreNames = new();
        [ObservableProperty] private string? _selectedGenreName;
        private int? _selectedGenreId;

        // Venue
        [ObservableProperty] private ObservableCollection<string> _venueNames = new();
        [ObservableProperty] private string? _selectedVenueName;

        // Tambah venue baru
        [ObservableProperty] private bool _isAddingVenue;
        [ObservableProperty] private string _newVenueName = "";
        [ObservableProperty] private string _newVenueCity = "";
        [ObservableProperty] private string _newVenueAddress = "";
        [ObservableProperty] private string _newVenueCapacity = "";

        // Jadwal
        [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(14);
        [ObservableProperty] private TimeSpan _startTime = new(19, 0, 0);
        [ObservableProperty] private DateTime _endDate = DateTime.Today.AddDays(14);
        [ObservableProperty] private TimeSpan _endTime = new(23, 0, 0);

        // Harga, kapasitas, lokasi
        [ObservableProperty] private string _minPriceText = "";
        [ObservableProperty] private string _maxPriceText = "";
        [ObservableProperty] private string _capacityText = "";
        [ObservableProperty] private string _latText = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
        [ObservableProperty] private string _lngText = DefaultLongitude.ToString(CultureInfo.InvariantCulture);

        public List<string> StatusOptions { get; } = new() { "Published", "Draft" };
        [ObservableProperty] private string _selectedStatus = "Published";

        public CreateEventViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsLoading = true;
            StatusMessage = "";

            try
            {
                _api.SetAuthToken(_auth.GetToken());

                var venuesTask = _api.GetVenuesAsync();
                var genresTask = _api.GetGenresAsync();
                await Task.WhenAll(venuesTask, genresTask);

                _venues = venuesTask.Result;
                _genres = genresTask.Result;

                VenueNames = new ObservableCollection<string>(_venues.Select(v => v.Name));
                GenreNames = new ObservableCollection<string>(_genres.Select(g => g.Name));
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal memuat data: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedGenreNameChanged(string? value)
        {
            _selectedGenreId = _genres.FirstOrDefault(g => g.Name == value)?.GenreId;
        }

        partial void OnSelectedVenueNameChanged(string? value)
        {
            _selectedVenue = _venues.FirstOrDefault(v => v.Name == value);
            ApplyVenueCoordinates(_selectedVenue);
        }

        private void ApplyVenueCoordinates(Venue? venue)
        {
            if (venue == null) return;

            if (venue.Latitude != 0 || venue.Longitude != 0)
            {
                LatText = venue.Latitude.ToString(CultureInfo.InvariantCulture);
                LngText = venue.Longitude.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // Venue tanpa koordinat → default pusat Jakarta agar muncul di Map.
                LatText = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
                LngText = DefaultLongitude.ToString(CultureInfo.InvariantCulture);
            }
        }

        [RelayCommand]
        private void ToggleAddVenue() => IsAddingVenue = !IsAddingVenue;

        [RelayCommand]
        private async Task SaveVenueAsync()
        {
            if (string.IsNullOrWhiteSpace(NewVenueName))
            {
                await Alerts.ShowAsync("Validasi", "Nama venue wajib diisi");
                return;
            }

            var name = NewVenueName.Trim();

            // Venue dengan nama sama sudah ada → langsung pakai itu.
            var existing = _venues.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                SelectedVenueName = existing.Name;
                IsAddingVenue = false;
                await Alerts.ShowAsync("Venue", $"Venue \"{existing.Name}\" sudah ada — dipilih otomatis.");
                return;
            }

            var capacity = 0;
            if (!string.IsNullOrWhiteSpace(NewVenueCapacity))
            {
                if (!int.TryParse(CleanNumber(NewVenueCapacity), out capacity) || capacity < 0)
                {
                    await Alerts.ShowAsync("Validasi", "Kapasitas venue tidak valid (angka, minimal 0)");
                    return;
                }
            }

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message, venue) = await _api.CreateVenueAsync(
                    name, NewVenueCity.Trim(), NewVenueAddress.Trim(), capacity);

                if (!success || venue == null)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                _venues.Add(venue);
                VenueNames.Add(venue.Name);
                SelectedVenueName = venue.Name;
                IsAddingVenue = false;
                NewVenueName = "";
                NewVenueCity = "";
                NewVenueAddress = "";
                NewVenueCapacity = "";

                await Alerts.ShowAsync("Berhasil", message);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task SaveEventAsync()
        {
            if (IsSaving) return;

            var validation = Validate();
            if (validation != null)
            {
                await Alerts.ShowAsync("Validasi", validation);
                return;
            }

            IsSaving = true;
            StatusMessage = "";
            try
            {
                var start = StartDate.Date + StartTime;
                var end = EndDate.Date + EndTime;
                var minPrice = ParsePrice(MinPriceText);
                var maxPrice = ParsePrice(MaxPriceText);

                var data = new CreateEventData
                {
                    Name = EventName.Trim(),
                    Description = EventDescription.Trim(),
                    PosterUrl = PosterUrl.Trim(),
                    TicketLink = TicketLink.Trim(),
                    VenueId = _selectedVenue?.VenueId,
                    GenreId = _selectedGenreId,
                    StartDate = start,
                    EndDate = end,
                    Latitude = double.Parse(LatText.Replace(',', '.'), CultureInfo.InvariantCulture),
                    Longitude = double.Parse(LngText.Replace(',', '.'), CultureInfo.InvariantCulture),
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Capacity = int.Parse(CleanNumber(CapacityText), CultureInfo.InvariantCulture),
                    Status = SelectedStatus
                };

                _api.SetAuthToken(_auth.GetToken());
                var (success, message, eventId) = await _api.CreateEventAsync(data);

                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                await Alerts.ShowAsync("Berhasil 🎉",
                    $"{message}\n\nEvent sekarang tampil di halaman \"Event Saya\" dan Discover. " +
                    "Jangan lupa tambahkan tipe tiket (Festival/Tribun/dll) lewat menu Kelola Tiket Event.");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private string? Validate()
        {
            if (string.IsNullOrWhiteSpace(EventName) || EventName.Trim().Length < 3)
                return "Nama event wajib diisi (minimal 3 karakter).";

            if (SelectedVenueName == null)
                return "Pilih venue dari daftar, atau tambahkan venue baru terlebih dahulu.";

            // Pengguna mulai mengisi venue baru tetapi belum menyimpannya.
            if (IsAddingVenue && !string.IsNullOrWhiteSpace(NewVenueName))
                return "Venue baru belum tersimpan — ketuk \"Simpan Venue\" terlebih dahulu.";

            var start = StartDate.Date + StartTime;
            var end = EndDate.Date + EndTime;
            if (end < start)
                return "Waktu selesai tidak boleh sebelum waktu mulai.";

            if (string.IsNullOrWhiteSpace(MinPriceText) || !decimal.TryParse(CleanNumber(MinPriceText), out var minPrice) || minPrice < 0)
                return "Harga minimum tidak valid (angka, minimal 0).";
            if (string.IsNullOrWhiteSpace(MaxPriceText) || !decimal.TryParse(CleanNumber(MaxPriceText), out var maxPrice) || maxPrice < 0)
                return "Harga maksimum tidak valid (angka, minimal 0).";
            if (minPrice > maxPrice)
                return "Harga minimum tidak boleh lebih besar dari harga maksimum.";

            if (!int.TryParse(CleanNumber(CapacityText), out var capacity) || capacity <= 0)
                return "Kapasitas penonton tidak valid (angka lebih dari 0).";

            if (string.IsNullOrWhiteSpace(LatText) ||
                !double.TryParse(LatText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                return "Koordinat Latitude tidak valid.";
            if (string.IsNullOrWhiteSpace(LngText) ||
                !double.TryParse(LngText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                return "Koordinat Longitude tidak valid.";

            return null;
        }

        private static decimal ParsePrice(string input) =>
            decimal.TryParse(CleanNumber(input), out var value) ? value : 0m;

        private static string CleanNumber(string input)
        {
            return (input ?? "")
                .Replace("Rp", "")
                .Replace("rp", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "")
                .Trim();
        }
    }
}
