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
    /// Tab "Dashboard" ArtistShell (GIGRADAR_ROLE_SYSTEM.md §7) — statistik karier
    /// dari GET /api/artist/me/dashboard, gig mendatang (tap → detail event),
    /// dan edit profil artist via PUT /api/artist/me.
    /// </summary>
    public partial class ArtistDashboardViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private bool _navigating;

        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _userRole = "Artist";
        [ObservableProperty] private ArtistDashboard? _dashboard;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRefreshing;
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private string _errorMessage = "";
        [ObservableProperty] private ObservableCollection<ArtistGig> _upcomingGigs = new();
        [ObservableProperty] private bool _hasGigs;

        // Form edit profil artist
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private string _formName = "";
        [ObservableProperty] private string _formGenre = "";
        [ObservableProperty] private string _formCity = "";
        [ObservableProperty] private string _formBio = "";
        [ObservableProperty] private string _formPhotoUrl = "";

        public string Initials => BuildInitials(UserName);

        public string FollowersLabel => FormatCount(Dashboard?.FollowersCount ?? 0);
        public string TracksLabel => (Dashboard?.TracksCount ?? 0).ToString();
        public string AlbumsLabel => (Dashboard?.AlbumsCount ?? 0).ToString();
        public string PostsLabel => (Dashboard?.PostsCount ?? 0).ToString();

        public string GenreLabel => string.IsNullOrWhiteSpace(Dashboard?.Genre)
            ? "Genre belum diatur"
            : Dashboard!.Genre;
        public string CityLabel => string.IsNullOrWhiteSpace(Dashboard?.City)
            ? "Kota belum diatur"
            : Dashboard!.City;

        public string GigsCountLabel => UpcomingGigs.Count switch
        {
            0 => "",
            1 => "1 gig mendatang",
            _ => $"{UpcomingGigs.Count} gig mendatang"
        };

        public ArtistDashboardViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                UserName = _auth.GetUserName();
                UserRole = _auth.GetUserRole();
                OnPropertyChanged(nameof(Initials));

                var data = await _api.GetArtistDashboardAsync();
                Dashboard = data;
                UpcomingGigs = new ObservableCollection<ArtistGig>(data?.UpcomingGigs ?? new List<ArtistGig>());
                HasGigs = UpcomingGigs.Count > 0;

                OnPropertyChanged(nameof(FollowersLabel));
                OnPropertyChanged(nameof(TracksLabel));
                OnPropertyChanged(nameof(AlbumsLabel));
                OnPropertyChanged(nameof(PostsLabel));
                OnPropertyChanged(nameof(GenreLabel));
                OnPropertyChanged(nameof(CityLabel));
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Gagal memuat dashboard: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
            if (IsEditing)
            {
                // Prefill dari data terakhir supaya tidak mengosongkan isian pengguna.
                FormName = Dashboard?.Name ?? UserName;
                FormGenre = Dashboard?.Genre ?? "";
                FormCity = Dashboard?.City ?? "";
                FormBio = "";
                FormPhotoUrl = Dashboard?.PhotoUrl ?? "";
            }
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(FormName))
            {
                await Alerts.ShowAsync("Validasi", "Nama artist wajib diisi");
                return;
            }

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                // coverUrl & socialLinks dikirim null → nilai lama dipertahankan server.
                // Bio juga null saat kosong: endpoint dashboard tidak mengembalikan bio,
                // jadi string kosong jangan sampai menimpa bio lama.
                var (success, message) = await _api.UpdateMyArtistProfileAsync(
                    FormName.Trim(),
                    string.IsNullOrWhiteSpace(FormBio) ? null : FormBio,
                    FormGenre, FormCity, FormPhotoUrl, null, null);

                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return; // Form tetap terbuka, isian dipertahankan.
                }

                _auth.UpdateStoredName(FormName.Trim());
                UserName = FormName.Trim();
                OnPropertyChanged(nameof(Initials));

                IsEditing = false;
                await Alerts.ShowAsync("Berhasil", message);
                await LoadCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>Buka detail event penuh dari baris gig (pola yang sama dengan ArtistDetailViewModel).</summary>
        [RelayCommand]
        private async Task GoToGigAsync(ArtistGig? gig)
        {
            if (gig == null || _navigating) return;
            _navigating = true;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var fullEvent = await _api.GetEventAsync(gig.EventId);

                if (fullEvent == null)
                {
                    await Alerts.ShowAsync("Info", "Detail event tidak ditemukan.");
                    return;
                }

                await Shell.Current.GoToAsync(nameof(EventDetailPage),
                    new Dictionary<string, object> { { "Event", fullEvent } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", $"Gagal membuka detail event: {ex.Message}");
            }
            finally
            {
                _navigating = false;
            }
        }

        private static string BuildInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }

        private static string FormatCount(int count)
            => count >= 1000 ? $"{count / 1000f:0.#}rb" : count.ToString();
    }
}