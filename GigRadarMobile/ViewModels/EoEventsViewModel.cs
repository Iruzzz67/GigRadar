using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Events" (EOShell/AdminShell) — daftar event yang dikelola
    /// (EO: event miliknya, Admin: semua) + aksi per event.
    /// </summary>
    public partial class EoEventsViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private EoDashboard? _dashboard;
        [ObservableProperty] private bool _hasEvents;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "";

        public EoEventsViewModel(ApiService api, AuthService auth)
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
                Dashboard = await _api.GetEoDashboardAsync();
                HasEvents = Dashboard?.Events is { Count: > 0 };
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

        [RelayCommand]
        private async Task MarkCompletedAsync(EoEventStat? item)
        {
            if (item == null) return;

            var ok = await Alerts.ConfirmAsync("Tandai Selesai",
                $"Tandai \"{item.Name}\" sebagai event yang sudah selesai?\nEvent tidak lagi menerima pembelian tiket.");
            if (!ok) return;

            await SetStatusAsync(item, "Completed");
        }

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