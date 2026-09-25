using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Halaman Peserta event untuk EO (GIGRADAR_APP_SPEC.md Bab 14/§30): ringkasan
    /// kehadiran + daftar peserta, bisa dicari nama, dan jalan pintas ke check-in.
    /// </summary>
    [QueryProperty(nameof(GigEvent), "Event")]
    public partial class AttendeesViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private List<EventAttendee> _all = new();

        [ObservableProperty]
        public partial GigEvent? GigEvent { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<EventAttendee> Attendees { get; set; } = new();
        [ObservableProperty]
        public partial string SearchText { get; set; } = "";
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }
        [ObservableProperty]
        public partial bool HasError { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasAttendees { get; set; }

        [ObservableProperty]
        public partial int Total { get; set; }
        [ObservableProperty]
        public partial int CheckedIn { get; set; }
        [ObservableProperty]
        public partial int Active { get; set; }
        [ObservableProperty]
        public partial string SummaryLabel { get; set; } = "";

        public string EventName => GigEvent?.Name ?? "Peserta";
        public string EventMeta => GigEvent == null
            ? string.Empty
            : $"{GigEvent.VenueName} • {GigEvent.DateFormatted}";

        /// <summary>Empty state dibedakan: tidak ada peserta vs hasil pencarian kosong.</summary>
        public bool ShowEmptyState => !HasAttendees && !HasError && !IsLoading;
        public string EmptyMessage => _all.Count > 0
            ? "Tidak ada peserta yang cocok dengan pencarian."
            : "Belum ada tiket terjual untuk event ini.";

        public AttendeesViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        partial void OnGigEventChanged(GigEvent? value)
        {
            OnPropertyChanged(nameof(EventName));
            OnPropertyChanged(nameof(EventMeta));
        }

        partial void OnSearchTextChanged(string value) => ApplySearch();

        partial void OnHasAttendeesChanged(bool value) => OnPropertyChanged(nameof(ShowEmptyState));
        partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(ShowEmptyState));
        partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(ShowEmptyState));

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (GigEvent == null) return;

            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var report = await _api.GetEventAttendeesAsync(GigEvent.EventId);
                if (report == null)
                {
                    HasError = true;
                    ErrorMessage = "Daftar peserta tidak bisa dimuat.";
                    return;
                }

                _all = report.Attendees;
                Total = report.Total;
                CheckedIn = report.CheckedIn;
                Active = report.Active;
                SummaryLabel = $"{CheckedIn} check-in · {Active} belum · {Total} tiket";

                ApplySearch();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Gagal memuat peserta: " + ex.Message;
                Attendees = new ObservableCollection<EventAttendee>();
                HasAttendees = false;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void ApplySearch()
        {
            var query = SearchText?.Trim() ?? "";

            var filtered = string.IsNullOrEmpty(query)
                ? _all
                : _all.Where(a =>
                    a.BuyerName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.TicketType.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            Attendees = new ObservableCollection<EventAttendee>(filtered);
            HasAttendees = Attendees.Count > 0;
            OnPropertyChanged(nameof(EmptyMessage));
        }

        /// <summary>Buka halaman check-in untuk event yang sama.</summary>
        [RelayCommand]
        private async Task OpenCheckInAsync()
        {
            if (GigEvent == null) return;

            await Shell.Current.GoToAsync(nameof(Views.CheckInPage),
                new Dictionary<string, object> { { "Event", GigEvent } });
        }
    }
}
