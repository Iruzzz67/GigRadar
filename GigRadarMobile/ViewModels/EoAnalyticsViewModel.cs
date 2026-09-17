using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Analytics" (EOShell) — GIGRADAR_APP_SPEC.md Bab 69 (STEP 21):
    /// ringkasan total (event, tiket, pendapatan, rata-rata konversi) +
    /// funnel per event (terjual, revenue, konversi, favorit, line-up)
    /// dari GET /api/analytics/eo.
    /// </summary>
    public partial class EoAnalyticsViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";

        [ObservableProperty]
        public partial ObservableCollection<EoEventAnalytics> Events { get; set; } = new();

        [ObservableProperty]
        public partial bool HasData { get; set; }

        // Ringkasan total (Bab 69 — EO: ticket sold, revenue, conversion)
        [ObservableProperty]
        public partial int TotalEvents { get; set; }

        [ObservableProperty]
        public partial int TotalTicketsSold { get; set; }

        [ObservableProperty]
        public partial string TotalRevenue { get; set; } = "Rp 0";

        [ObservableProperty]
        public partial string AvgConversion { get; set; } = "0%";

        [ObservableProperty]
        public partial int TotalFavorites { get; set; }

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

                var events = await _api.GetEoAnalyticsAsync();
                Events = new ObservableCollection<EoEventAnalytics>(events);
                HasData = Events.Count > 0;

                TotalEvents = Events.Count;
                TotalTicketsSold = Events.Sum(e => e.TicketsSold);
                TotalRevenue = $"Rp {Events.Sum(e => e.Revenue):N0}";
                TotalFavorites = Events.Sum(e => e.FavoritedCount);
                var avg = Events.Count > 0 ? Events.Average(e => e.ConversionRate) : 0;
                AvgConversion = $"{avg:P0}";
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal memuat analytics: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}