using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Halaman profil Event Organizer: edit data diri + ringkasan event miliknya
    /// (total event, tiket terjual, pendapatan, dan daftar event per statistik).
    /// </summary>
    public partial class EoProfileViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _userEmail = "";
        [ObservableProperty] private string _userRole = "";
        [ObservableProperty] private string _userCity = "";
        [ObservableProperty] private string _userPhotoUrl = "";
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "";
        [ObservableProperty] private EoDashboard? _dashboard;
        [ObservableProperty] private bool _hasEvents;

        public string RoleBadge => UserRole.ToUpperInvariant();
        public Color RoleBadgeColor => string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase)
            ? Color.FromArgb("#39FF14")
            : Color.FromArgb("#7B2FFF");

        public EoProfileViewModel(ApiService api, AuthService auth)
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
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                UserName = _auth.GetUserName();
                UserEmail = _auth.GetUserEmail();
                UserRole = _auth.GetUserRole();
                OnPropertyChanged(nameof(RoleBadge));
                OnPropertyChanged(nameof(RoleBadgeColor));

                // Data diri terbaru dari server
                var profile = await _api.GetProfileAsync();
                if (profile != null)
                {
                    UserName = profile.Name;
                    UserEmail = profile.Email;
                    UserRole = profile.Role;
                    UserCity = profile.City;
                    UserPhotoUrl = profile.PhotoUrl;
                    OnPropertyChanged(nameof(RoleBadge));
                    OnPropertyChanged(nameof(RoleBadgeColor));
                }

                var summary = await _api.GetEoDashboardAsync();
                Dashboard = summary;
                HasEvents = summary?.Events is { Count: > 0 };
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

        [RelayCommand]
        private void ToggleEdit() => IsEditing = !IsEditing;

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                await _api.UpdateProfileAsync(UserName, UserCity, UserPhotoUrl);
                _auth.UpdateStoredName(UserName);
                IsEditing = false;
                await Alerts.ShowAsync("Berhasil", "Data diri tersimpan!");
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task GoToManageTicketsAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(ManageTicketsPage));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task GoToCreateEventAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(CreateEventPage));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        // ── Aksi per event (Event Saya) ──────────────────

        /// <summary>Aksi utama: tandai tiket habis (saat Aktif) atau aktifkan lagi (saat SoldOut/Completed/Draft).</summary>
        [RelayCommand]
        private async Task TogglePrimaryActionAsync(EoEventStat? item)
        {
            if (item == null) return;

            var target = item.IsActive ? "SoldOut" : "Published";
            var message = item.IsActive
                ? $"Tandai \"{item.Name}\" sebagai tiket habis (sold out)?\nPembeli tidak akan bisa membeli tiket event ini lagi."
                : $"Aktifkan kembali \"{item.Name}\"? Tiket event ini bisa dibeli lagi.";

            var ok = await Alerts.ConfirmAsync("Ubah Status Event", message);
            if (!ok) return;

            await SetStatusAsync(item, target);
        }

        /// <summary>Menandai event sudah selesai (Completed).</summary>
        [RelayCommand]
        private async Task MarkCompletedAsync(EoEventStat? item)
        {
            if (item == null) return;

            var ok = await Alerts.ConfirmAsync("Tandai Selesai",
                $"Tandai \"{item.Name}\" sebagai event yang sudah selesai?\nEvent tidak lagi menerima pembelian tiket.");
            if (!ok) return;

            await SetStatusAsync(item, "Completed");
        }

        /// <summary>Hapus event beserta tiket &amp; data terkaitnya.</summary>
        [RelayCommand]
        private async Task DeleteEventAsync(EoEventStat? item)
        {
            if (item == null) return;

            var ok = await Alerts.ConfirmAsync("Hapus Event",
                $"Hapus \"{item.Name}\" secara permanen?\n\nTiket yang terjual, favorit, dan data terkait event ini ikut terhapus. Tindakan ini tidak bisa dibatalkan.",
                accept: "Ya, Hapus", cancel: "Batal");
            if (!ok) return;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = await _api.DeleteEventAsync(item.EventId);
                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                await Alerts.ShowAsync("Berhasil", $"{message}\nEvent \"{item.Name}\" sudah dihapus.");
                await LoadCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        private async Task SetStatusAsync(EoEventStat item, string status)
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = await _api.UpdateEventStatusAsync(item.EventId, status);
                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                await Alerts.ShowAsync("Berhasil", message);
                await LoadCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }
    }
}