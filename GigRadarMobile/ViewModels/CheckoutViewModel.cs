using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(GigEvent), "Event")]
    [QueryProperty(nameof(SelectedType), "Type")]
    public partial class CheckoutViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private GigEvent? _gigEvent;
        [ObservableProperty] private EventTicketType? _selectedType;

        // Data diri pembeli
        [ObservableProperty] private string _fullName;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _email;
        [ObservableProperty] private DateTime _birthDate = new(2000, 1, 1);

        [ObservableProperty] private bool _isProcessing;

        public const int MinimumAge = 17;

        public CheckoutViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;

            // Prefill dari akun (bisa diedit pembeli)
            _fullName = _auth.GetUserName() == "Guest" ? string.Empty : _auth.GetUserName();
            _email = _auth.GetUserEmail();
        }

        public string EventName => GigEvent?.Name ?? "";
        public string EventDate => GigEvent?.DateFormatted ?? "";
        public string VenueName => GigEvent?.VenueName ?? "";
        public string TypeName => SelectedType?.Name ?? "";
        public string PriceFormatted => SelectedType?.PriceFormatted ?? "";

        partial void OnGigEventChanged(GigEvent? value) => OnSummaryChanged();
        partial void OnSelectedTypeChanged(EventTicketType? value) => OnSummaryChanged();

        private void OnSummaryChanged()
        {
            OnPropertyChanged(nameof(EventName));
            OnPropertyChanged(nameof(EventDate));
            OnPropertyChanged(nameof(VenueName));
            OnPropertyChanged(nameof(TypeName));
            OnPropertyChanged(nameof(PriceFormatted));
        }

        [RelayCommand]
        private async Task PayAsync()
        {
            if (IsProcessing) return;

            var validation = ValidateBuyer();
            if (validation != null)
            {
                await Alerts.ShowAsync("Verifikasi Gagal", validation);
                return;
            }

            IsProcessing = true;
            try
            {
                _api.SetAuthToken(_auth.GetToken());

                var (success, message, ticket) = await _api.PurchaseTicketAsync(
                    GigEvent!.EventId,
                    SelectedType!.EventTicketTypeId,
                    FullName.Trim(),
                    Phone.Trim(),
                    Email.Trim(),
                    BirthDate);

                if (success && ticket != null)
                {
                    await Shell.Current.GoToAsync(nameof(TicketSuccessPage),
                        new Dictionary<string, object> { { "Ticket", ticket } });
                }
                else
                {
                    await Alerts.ShowAsync("Pembelian Gagal", message);
                }
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Verifikasi data diri: nama, telepon, email, dan umur minimal — menentukan
        /// apakah pembeli diperbolehkan membeli tiket.
        /// </summary>
        private string? ValidateBuyer()
        {
            if (string.IsNullOrWhiteSpace(FullName) || FullName.Trim().Length < 3)
                return "Nama lengkap wajib diisi (minimal 3 karakter).";

            var digits = new string(Phone.Where(char.IsDigit).ToArray());
            if (digits.Length < 9)
                return "Nomor telepon tidak valid (minimal 9 digit).";

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') || !Email.Contains('.'))
                return "Format email tidak valid.";

            if (BirthDate == default)
                return "Tanggal lahir wajib diisi.";

            var age = CalculateAge(BirthDate);
            if (age < MinimumAge)
                return $"Maaf, kamu belum memenuhi syarat umur minimal {MinimumAge} tahun untuk membeli tiket ini.";

            return null;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}