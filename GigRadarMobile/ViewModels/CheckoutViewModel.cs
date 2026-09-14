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

        // Error inline per field (UX guideline #8: error dekat field, bukan hanya alert)
        [ObservableProperty] private string _fullNameError = "";
        [ObservableProperty] private string _phoneError = "";
        [ObservableProperty] private string _emailError = "";
        [ObservableProperty] private string _birthDateError = "";

        [ObservableProperty] private bool _isProcessing;

        public const int MinimumAge = CheckoutValidators.MinimumAge;

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

        // Kosongkan error saat pengguna memperbaiki isian.
        partial void OnFullNameChanged(string value) => FullNameError = "";
        partial void OnPhoneChanged(string value) => PhoneError = "";
        partial void OnEmailChanged(string value) => EmailError = "";
        partial void OnBirthDateChanged(DateTime value) => BirthDateError = "";

        [RelayCommand]
        private async Task PayAsync()
        {
            if (IsProcessing) return;

            if (!ValidateBuyer()) return;

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
        /// Verifikasi data diri: nama, telepon, email, dan umur minimal. Error ditampilkan
        /// inline di bawah masing-masing field; mengembalikan true bila semua valid.
        /// </summary>
        private bool ValidateBuyer()
        {
            var isValid = true;

            FullNameError = CheckoutValidators.IsValidFullName(FullName)
                ? ""
                : "Nama wajib diisi (minimal 3 karakter).";
            if (FullNameError.Length > 0) isValid = false;

            PhoneError = CheckoutValidators.IsValidPhone(Phone)
                ? ""
                : "Nomor telepon tidak valid (minimal 9 digit).";
            if (PhoneError.Length > 0) isValid = false;

            EmailError = CheckoutValidators.IsValidEmail(Email)
                ? ""
                : "Format email tidak valid.";
            if (EmailError.Length > 0) isValid = false;

            var age = CheckoutValidators.CalculateAge(BirthDate);
            BirthDateError = age < MinimumAge
                ? $"Umur minimal {MinimumAge} tahun untuk membeli tiket ini."
                : "";
            if (BirthDateError.Length > 0) isValid = false;

            return isValid;
        }
    }
}