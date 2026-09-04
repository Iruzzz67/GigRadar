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

        [ObservableProperty] private GigEvent? _gigEvent;
        [ObservableProperty] private Artist? _selectedArtist;
        [ObservableProperty] private string _playbackStatus = "Tap to play preview";
        [ObservableProperty] private bool _isFavorited;

        /// <summary>Teks tombol beli — menyesuaikan status event (habis/selesai/belum tersedia).</summary>
        public string BuyButtonText => GigEvent?.Status switch
        {
            "SoldOut" => "Tiket Habis",
            "Completed" => "Event Selesai",
            "Draft" => "Belum Tersedia",
            _ => "🎫 Buy Ticket"
        };

        partial void OnGigEventChanged(GigEvent? value) => OnPropertyChanged(nameof(BuyButtonText));

        public EventDetailViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task PlayPreviewAsync(Artist? artist)
        {
            if (artist == null || artist.Tracks.Count == 0)
            {
                PlaybackStatus = "No preview available";
                return;
            }

            SelectedArtist = artist;
            var track = artist.Tracks.First();

            if (string.IsNullOrWhiteSpace(track.AudioUrl))
            {
                PlaybackStatus = "No preview available";
                return;
            }

            PlaybackStatus = $"Playing: {track.Title}";
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
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
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
    }
}
