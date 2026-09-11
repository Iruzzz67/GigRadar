using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _userEmail = "";
        [ObservableProperty] private string _userCity = "";
        [ObservableProperty] private bool _isEditing;
        [ObservableProperty] private bool _isStaff;

        public string Initials => BuildInitials(UserName);

        private static string BuildInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }

        public ProfileViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadProfileAsync()
        {
            UserName = _auth.GetUserName();
            UserEmail = _auth.GetUserEmail();
            UserCity = Preferences.Default.Get("user_city", "Jakarta");
            var role = _auth.GetUserRole();
            IsStaff = role is "EO" or "Admin";
            OnPropertyChanged(nameof(Initials));
        }

        [RelayCommand]
        private async Task GoToEoProfileAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(EoProfilePage));
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
        private async Task SaveProfileAsync()
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                await _api.UpdateProfileAsync(UserName, UserCity);

                // Sinkronkan kota + nama ke storage lokal supaya Home/Radar ikut berubah.
                Preferences.Default.Set("user_city", UserCity);
                _auth.UpdateStoredName(UserName);

                IsEditing = false;
                OnPropertyChanged(nameof(Initials));
                await Alerts.ShowAsync("Success", "Profil tersimpan!");
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _auth.Logout();
            NavigationHelper.SetRoot(new NavigationPage(
                new LoginPage(App.ServiceProvider.GetRequiredService<ViewModels.LoginViewModel>())));
        }
    }
}
