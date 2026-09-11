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

        private const double DefaultLat = -6.2088;
        private const double DefaultLng = 106.8456;

        [ObservableProperty] private GigEvent? _gigEvent;
        [ObservableProperty] private Artist? _selectedArtist;
        [ObservableProperty] private string _playbackStatus = "Tap untuk preview lagu";
        [ObservableProperty] private bool _isFavorited;
        [ObservableProperty] private string _favoriteLabel = "Simpan";
        [ObservableProperty] private string _distanceLabel = "";
        [ObservableProperty] private bool _isBuyable;
        [ObservableProperty] private ObservableCollection<EventTicketType> _ticketTypes = new();
        [ObservableProperty] private bool _hasTicketTypes;

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

            if (value == null) return;

            IsBuyable = value.Status == "Published";
            DistanceLabel = $"{GeoHelper.FormatKm(GeoHelper.HaversineKm(DefaultLat, DefaultLng, value.Latitude, value.Longitude))} dari kamu";

            // Muat tipe tiket (hanya bila tidak dijual via link eksternal).
            if (!value.HasExternalLink)
            {
                _ = LoadTicketTypesAsync(value.EventId);
            }
        }

        public EventDetailViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
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
            OnPropertyChanged(nameof(ShowPriceFallback));
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
                await _api.ToggleFavoriteAsync(GigEvent.EventId);
                IsFavorited = !IsFavorited;
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