using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Analytics" (EOShell) — statistik penjualan per event milik EO
    /// dari GET /api/events/managed/summary.
    /// </summary>
    public partial class EoAnalyticsViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private EoDashboard? _dashboard;
        [ObservableProperty] private bool _hasData;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "";

        public EoAnalyticsViewModel(ApiService api, AuthService auth)
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
                HasData = Dashboard?.Events is { Count: > 0 };
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
    }
}