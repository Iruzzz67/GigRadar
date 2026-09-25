using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(GigEvent), "Event")]
    public partial class EventDetailViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private readonly LocationService _locationService;

        // Fallback pusat Jakarta bila lokasi perangkat tidak tersedia.
        private double _userLat = -6.2088;
        private double _userLng = 106.8456;

        [ObservableProperty]
        public partial GigEvent? GigEvent { get; set; }
        [ObservableProperty]
        public partial Artist? SelectedArtist { get; set; }
        [ObservableProperty]
        public partial string PlaybackStatus { get; set; } = "Tap untuk preview lagu";
        [ObservableProperty]
        public partial bool IsFavorited { get; set; }
        [ObservableProperty]
        public partial string FavoriteLabel { get; set; } = "Simpan";
        [ObservableProperty]
        public partial string DistanceLabel { get; set; } = "";
        [ObservableProperty]
        public partial bool IsBuyable { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<EventTicketType> TicketTypes { get; set; } = new();
        [ObservableProperty]
        public partial bool HasTicketTypes { get; set; }

        /// <summary>Info di sticky bar: kenapa pembelian belum bisa dilakukan.</summary>
        [ObservableProperty]
        public partial string TicketHint { get; set; } = "";

        /// <summary>Buy hanya aktif bila event aktif DAN ada tipe tiket ATAU link eksternal.</summary>
        public bool CanBuy => GigEvent != null && GigEvent.Status == "Published"
            && (GigEvent.HasExternalLink || HasTicketTypes);

        public string BuyButtonText
        {
            get
            {
                if (GigEvent == null) return "Beli Tiket";
                return GigEvent.Status switch
                {
                    "SoldOut" => "Tiket Habis",
                    "Completed" => "Event Selesai",
                    "Draft" => "Belum Tersedia",
                    _ => GigEvent.HasExternalLink ? "Beli di Loket" : "Beli Tiket"
                };
            }
        }

        public string PriceFromLabel
        {
            get
            {
                if (GigEvent == null) return "";
                return GigEvent.MinPrice == GigEvent.MaxPrice
                    ? GigEvent.PriceFormatted
                    : $"Mulai {GigEvent.MinPrice:N0}";
            }
        }

        public string PriceCaption => GigEvent?.MinPrice == GigEvent?.MaxPrice ? "Tiket" : "Tiket mulai dari";

        public bool ShowPriceFallback => GigEvent != null && !GigEvent.HasExternalLink && !HasTicketTypes;

        partial void OnGigEventChanged(GigEvent? value)
        {
            OnPropertyChanged(nameof(BuyButtonText));
            OnPropertyChanged(nameof(PriceFromLabel));
            OnPropertyChanged(nameof(PriceCaption));
            OnPropertyChanged(nameof(ShowPriceFallback));
            OnPropertyChanged(nameof(CanBuy));

            if (value == null) return;

            // Selama tipe tiket belum termuat, jangan izinkan pembelian.
            TicketHint = value.HasExternalLink ? "" : "Memuat ketersediaan tiket…";

            _ = LoadFavoriteStateAsync(value.EventId);

            IsBuyable = value.Status == "Published";
            // Event tanpa koordinat (0,0) tidak punya info jarak yang bermakna — jangan tampilkan "0 m dari kamu".
            DistanceLabel = (value.Latitude == 0 && value.Longitude == 0)
                ? ""
                : $"{GeoHelper.FormatKm(GeoHelper.HaversineKm(_userLat, _userLng, value.Latitude, value.Longitude))} dari kamu";

            // Muat tipe tiket (hanya bila tidak dijual via link eksternal).
            if (!value.HasExternalLink)
            {
                _ = LoadTicketTypesAsync(value.EventId);
            }
        }

        public EventDetailViewModel(ApiService api, AuthService auth, LocationService locationService)
        {
            _api = api;
            _auth = auth;
            _locationService = locationService;
            _ = InitLocationAsync();
        }

        /// <summary>Ambil lokasi perangkat sekali agar jarak akurat (fallback: pusat Jakarta).</summary>
        private async Task InitLocationAsync()
        {
            try
            {
                var location = await _locationService.GetLastKnownLocationAsync();
                if (location != null)
                {
                    _userLat = location.Latitude;
                    _userLng = location.Longitude;

                    // Hitung ulang jarak bila event sudah terlanjur tampil dengan fallback.
                    if (GigEvent != null)
                        OnGigEventChanged(GigEvent);
                }
            }
            catch { /* lokasi opsional — fallback Jakarta */ }
        }

        /// <summary>Muat status favorit event ini (agar ♡ tidak mulai kosong padahal tersimpan).</summary>
        private async Task LoadFavoriteStateAsync(int eventId)
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var favorites = await _api.GetFavoritesAsync();
                IsFavorited = favorites.Any(f => f.EventId == eventId);
            }
            catch { /* favorit opsional — biarkan default false */ }
        }

        private async Task LoadTicketTypesAsync(int eventId)
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var types = await _api.GetEventTicketTypesAsync(eventId);
                TicketTypes = new ObservableCollection<EventTicketType>(types);
                HasTicketTypes = TicketTypes.Count > 0;
            }
            catch
            {
                HasTicketTypes = false;
            }

            TicketHint = HasTicketTypes || (GigEvent?.HasExternalLink ?? false)
                ? ""
                : "Tiket belum tersedia — EO event ini belum menerbitkan tipe tiket.";

            OnPropertyChanged(nameof(ShowPriceFallback));
            OnPropertyChanged(nameof(CanBuy));
        }

        /// <summary>
        /// Muat ulang event dari server saat halaman dibuka. Dua manfaat: view tercatat
        /// di server (sinyal popularitas untuk rekomendasi §11) dan status/stok tiket
        /// selalu segar. Bila gagal (offline), data yang sudah tampil tetap dipakai.
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (GigEvent == null) return;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var fresh = await _api.GetEventAsync(GigEvent.EventId);
                if (fresh != null)
                    GigEvent = fresh;
            }
            catch { /* data dari halaman sebelumnya tetap dipakai */ }
        }

        [RelayCommand]
        private async Task PlayPreviewAsync(Artist? artist)
        {
            if (artist == null || artist.Tracks.Count == 0)
            {
                PlaybackStatus = "Preview belum tersedia";
                return;
            }

            SelectedArtist = artist;
            var track = artist.Tracks.First();

            if (string.IsNullOrWhiteSpace(track.AudioUrl))
            {
                PlaybackStatus = "Preview belum tersedia";
                return;
            }

            PlaybackStatus = $"Memutar: {track.Title}";
            await Launcher.OpenAsync(track.AudioUrl);
        }

        [RelayCommand]
        private async Task ToggleFavoriteAsync()
        {
            if (GigEvent == null) return;
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                // State akhir dari server — bukan flip optimistis, agar ♡ selalu sinkron.
                IsFavorited = await _api.ToggleFavoriteAsync(GigEvent.EventId);
                FavoriteLabel = IsFavorited ? "Tersimpan" : "Simpan";
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task ShareAsync()
        {
            if (GigEvent == null) return;
            var text = $"{GigEvent.Name} — {GigEvent.DateFormatted} di {GigEvent.VenueName}. " +
                       $"{GigEvent.PriceFormatted}. Temukan di GIGRADAR.";
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = GigEvent.Name,
                Text = text
            });
        }

        [RelayCommand]
        private async Task OpenMapsAsync()
        {
            if (GigEvent == null) return;
            var url = $"https://www.google.com/maps?q={GigEvent.Latitude},{GigEvent.Longitude}";
            await Launcher.OpenAsync(url);
        }

        [RelayCommand]
        private async Task BuyTicketAsync()
        {
            if (GigEvent == null) return;

            try
            {
                // Event tidak aktif (tiket habis / selesai / draft) → tolak beli.
                if (GigEvent.Status != "Published")
                {
                    var message = GigEvent.Status == "SoldOut"
                        ? "Tiket untuk event ini sudah habis."
                        : "Event ini sudah selesai atau sedang tidak menerima pembelian tiket.";
                    await Alerts.ShowAsync("Info", message);
                    return;
                }

                // Bila venue/promotor hanya menyediakan link pembelian eksternal,
                // arahkan langsung ke link tersebut.
                if (GigEvent.HasExternalLink)
                {
                    await Launcher.OpenAsync(GigEvent.TicketLink);
                    return;
                }

                // Event aktif tetapi belum ada tipe tiket → jangan bawa ke halaman kosong.
                if (!HasTicketTypes)
                {
                    await Alerts.ShowAsync("Tiket belum tersedia",
                        "EO event ini belum menerbitkan tipe tiket, jadi pembelian belum bisa dilakukan.\n\n" +
                        "Kamu EO-nya? Buka tab Tickets → pilih event ini → \"Tambah Tipe Tiket\" (mis. Festival/Tribun).");
                    return;
                }

                // Jika tidak, tampilkan pilihan tipe tiket (Festival/Tribun/Bundling).
                await Shell.Current.GoToAsync(nameof(TicketSelectionPage),
                    new Dictionary<string, object> { { "Event", GigEvent } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task GoToArtistAsync(Artist artist)
        {
            if (artist == null) return;
            await Shell.Current.GoToAsync(nameof(ArtistDetailPage),
                new Dictionary<string, object> { { "Artist", artist } });
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
