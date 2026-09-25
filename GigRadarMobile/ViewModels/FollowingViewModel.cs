using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Halaman Following (GIGRADAR_APP_SPEC.md Bab 5/10): daftar artist yang diikuti
    /// user, aksi berhenti mengikuti, dan jalan pintas ke profil artist.
    /// </summary>
    public partial class FollowingViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private bool _navigating;

        [ObservableProperty]
        public partial ObservableCollection<FollowedArtist> Artists { get; set; } = new();
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
        public partial bool HasError { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasFollowedArtists { get; set; }
        [ObservableProperty]
        public partial string CountLabel { get; set; } = "";

        /// <summary>Empty state hanya tampil saat memang kosong — bukan saat pemuatan gagal.</summary>
        public bool ShowEmptyState => !HasFollowedArtists && !HasError;

        public FollowingViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
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
                _api.SetAuthToken(_auth.GetToken());
                var follows = await _api.GetFollowsAsync();
                Artists = new ObservableCollection<FollowedArtist>(follows);
                UpdateCounts();
            }
            catch (Exception ex)
            {
                // Error ditampilkan inline dengan tombol coba lagi — bukan empty state
                // yang menyesatkan (seolah user tidak mengikuti artist apa pun).
                HasError = true;
                ErrorMessage = "Gagal memuat daftar following: " + ex.Message;
                Artists = new ObservableCollection<FollowedArtist>();
                UpdateCounts();
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void UpdateCounts()
        {
            HasFollowedArtists = Artists.Count > 0;
            CountLabel = Artists.Count == 0 ? "" : $"{Artists.Count} artist diikuti";
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        /// <summary>Berhenti mengikuti artist — dikonfirmasi dulu karena mengubah daftar user.</summary>
        [RelayCommand]
        private async Task UnfollowAsync(FollowedArtist? artist)
        {
            if (artist == null) return;

            var ok = await Alerts.ConfirmAsync(
                "Berhenti mengikuti",
                $"Berhenti mengikuti {artist.ArtistName}? Kamu tidak akan melihat update mereka lagi.",
                accept: "Berhenti",
                cancel: "Batal");
            if (!ok) return;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = await _api.UnfollowArtistAsync(artist.ArtistId);
                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                // Server sudah menyetujui — baris baru dihapus dari daftar.
                Artists.Remove(artist);
                UpdateCounts();
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Buka profil artist. Daftar following hanya menyimpan ringkasan, jadi detail
        /// lengkap (track/gig/post) diambil dulu agar halaman profil tetap akurat.
        /// </summary>
        [RelayCommand]
        private async Task OpenArtistAsync(FollowedArtist? item)
        {
            if (item == null || _navigating) return;
            _navigating = true;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var artist = await _api.GetArtistAsync(item.ArtistId);

                if (artist == null)
                {
                    await Alerts.ShowAsync("Info", "Profil artist tidak ditemukan.");
                    return;
                }

                await Shell.Current.GoToAsync(nameof(ArtistDetailPage),
                    new Dictionary<string, object> { { "Artist", artist } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", $"Gagal membuka profil artist: {ex.Message}");
            }
            finally
            {
                _navigating = false;
            }
        }

        /// <summary>Empty state → ajak user menemukan artist baru di Explore.</summary>
        [RelayCommand]
        private async Task ExploreArtistsAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//explore");
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }
    }
}
