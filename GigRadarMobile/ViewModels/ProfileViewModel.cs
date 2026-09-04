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
            var role = _auth.GetUserRole();
            IsStaff = role is "EO" or "Admin";
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
                IsEditing = false;
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
