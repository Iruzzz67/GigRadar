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
        private readonly LocationService _locationService;

        private List<Venue> _venues = new();
        private List<Genre> _genres = new();
        private Venue? _selectedVenue;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsSaving { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";

        // Info event
        [ObservableProperty]
        public partial string EventName { get; set; } = "";
        [ObservableProperty]
        public partial string EventDescription { get; set; } = "";
        [ObservableProperty]
        public partial string PosterUrl { get; set; } = "";
        [ObservableProperty]
        public partial string PosterFileName { get; set; } = "";
        [ObservableProperty]
        public partial string TicketLink { get; set; } = "";

        // Genre (ditampilkan lewat daftar nama agar Picker sederhana)
        [ObservableProperty]
        public partial ObservableCollection<string> GenreNames { get; set; } = new();
        [ObservableProperty]
        public partial string? SelectedGenreName { get; set; }
        private int? _selectedGenreId;

        // Venue
        [ObservableProperty]
        public partial ObservableCollection<string> VenueNames { get; set; } = new();
        [ObservableProperty]
        public partial string? SelectedVenueName { get; set; }
        [ObservableProperty]
        public partial string SelectedVenueAddress { get; set; } = "";

        // Tambah venue baru
        [ObservableProperty]
        public partial bool IsAddingVenue { get; set; }
        [ObservableProperty]
        public partial string NewVenueName { get; set; } = "";
        [ObservableProperty]
        public partial string NewVenueCity { get; set; } = "";
        [ObservableProperty]
        public partial string NewVenueAddress { get; set; } = "";
        [ObservableProperty]
        public partial string NewVenueCapacity { get; set; } = "";
        [ObservableProperty]
        public partial string NewVenueLatText { get; set; } = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
        [ObservableProperty]
        public partial string NewVenueLngText { get; set; } = DefaultLongitude.ToString(CultureInfo.InvariantCulture);

        // Jadwal
        [ObservableProperty]
        public partial DateTime StartDate { get; set; } = DateTime.Today.AddDays(14);
        [ObservableProperty]
        public partial TimeSpan StartTime { get; set; } = new(19, 0, 0);
        [ObservableProperty]
        public partial DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
        [ObservableProperty]
        public partial TimeSpan EndTime { get; set; } = new(23, 0, 0);

        // Harga, kapasitas, lokasi
        [ObservableProperty]
        public partial string MinPriceText { get; set; } = "";
        [ObservableProperty]
        public partial string MaxPriceText { get; set; } = "";
        [ObservableProperty]
        public partial string CapacityText { get; set; } = "";
        [ObservableProperty]
        public partial string LatText { get; set; } = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
        [ObservableProperty]
        public partial string LngText { get; set; } = DefaultLongitude.ToString(CultureInfo.InvariantCulture);

        public List<string> StatusOptions { get; } = new() { "Published", "Draft" };
        [ObservableProperty]
        public partial string SelectedStatus { get; set; } = "Published";

        public CreateEventViewModel(ApiService api, AuthService auth, LocationService locationService)
        {
            _api = api;
            _auth = auth;
            _locationService = locationService;
        }

        [RelayCommand]
        private async Task PickPosterAsync()
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Pilih poster event",
                FileTypes = FilePickerFileType.Images
            });

            if (file == null)
                return;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension is not ".jpg" and not ".jpeg" and not ".png")
            {
                await Alerts.ShowAsync("Format tidak didukung", "Poster harus berupa foto JPG/JPEG atau PNG.");
                return;
            }

            await using var stream = await file.OpenReadAsync();
            if (stream.Length > 8 * 1024 * 1024)
            {
                await Alerts.ShowAsync("File terlalu besar", "Ukuran poster maksimal 8 MB.");
                return;
            }

            await using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            var mediaType = extension == ".png" ? "image/png" : "image/jpeg";
            PosterUrl = $"data:{mediaType};base64,{Convert.ToBase64String(buffer.ToArray())}";
            PosterFileName = file.FileName;
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
            SelectedVenueAddress = _selectedVenue == null
                ? ""
                : string.Join(", ", new[] { _selectedVenue.Address, _selectedVenue.City }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
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
        private async Task UseCurrentLocationAsync()
        {
            try
            {
                var location = await _locationService.GetBestAvailableLocationAsync();
                if (location != null)
                {
                    LatText = location.Latitude.ToString(CultureInfo.InvariantCulture);
                    LngText = location.Longitude.ToString(CultureInfo.InvariantCulture);
                    NewVenueLatText = LatText;
                    NewVenueLngText = LngText;
                    await Alerts.ShowAsync("Info", "Berhasil mengambil lokasi perangkat. Koordinat ini akan dipakai sebagai lokasi event.");
                }
                else
                {
                    await Alerts.ShowAsync("Peringatan", "Lokasi tidak tersedia atau tidak diizinkan.");
                }
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
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

            if (string.IsNullOrWhiteSpace(NewVenueAddress) || NewVenueAddress.Trim().Length < 8)
            {
                await Alerts.ShowAsync("Validasi", "Alamat lengkap venue wajib diisi (minimal 8 karakter).");
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

            if (!TryParseCoordinate(NewVenueLatText, -90, 90, out var latitude))
            {
                await Alerts.ShowAsync("Validasi", "Latitude venue tidak valid.");
                return;
            }

            if (!TryParseCoordinate(NewVenueLngText, -180, 180, out var longitude))
            {
                await Alerts.ShowAsync("Validasi", "Longitude venue tidak valid.");
                return;
            }

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message, venue) = await _api.CreateVenueAsync(
                    name, NewVenueCity.Trim(), NewVenueAddress.Trim(), capacity, latitude, longitude);

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
                NewVenueLatText = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
                NewVenueLngText = DefaultLongitude.ToString(CultureInfo.InvariantCulture);

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

            if (string.IsNullOrWhiteSpace(PosterUrl))
                return "Poster event wajib dipilih dalam format JPG/JPEG atau PNG.";

            if (!PosterUrl.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase) &&
                !PosterUrl.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
                return "Poster harus berupa file JPG/JPEG atau PNG, bukan URL.";

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
                !double.TryParse(LatText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                latitude is < -90 or > 90)
                return "Koordinat Latitude tidak valid.";
            if (string.IsNullOrWhiteSpace(LngText) ||
                !double.TryParse(LngText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) ||
                longitude is < -180 or > 180)
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

        private static bool TryParseCoordinate(string input, double min, double max, out double value)
        {
            return double.TryParse(input?.Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out value) &&
                !double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max;
        }
    }
}
