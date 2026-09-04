using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _name = "";
        [ObservableProperty] private bool _isRegister;
        [ObservableProperty] private bool _isLoading;

        public LoginViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegister = !IsRegister;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await Alerts.ShowAsync("Error", "Email dan password wajib diisi");
                return;
            }

            IsLoading = true;

            try
            {
                if (IsRegister)
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        await Alerts.ShowAsync("Error", "Nama wajib diisi");
                        return;
                    }

                    var (success, message, token, user) = await _api.RegisterAsync(Name, Email, Password);
                    if (!success || token == null)
                    {
                        await Alerts.ShowAsync("Error", message);
                        return;
                    }

                    _auth.SaveSession(token, user!.UserId, user.Name, user.Email, user.Role);
                    _api.SetAuthToken(token);
                    GoToOnboarding();
                }
                else
                {
                    var (success, message, token, user) = await _api.LoginAsync(Email, Password);
                    if (!success || token == null)
                    {
                        await Alerts.ShowAsync("Error", message);
                        return;
                    }

                    _auth.SaveSession(token, user!.UserId, user.Name, user.Email, user.Role);
                    _api.SetAuthToken(token);

                    // Routing berbasis role (§24): EO/Admin/Artist → shell masing-masing,
                    // User → onboarding genre (jika belum) lalu UserShell.
                    if (user.Role != "User")
                    {
                        NavigationHelper.SetRoot(ShellRouter.CreateForRole(user.Role));
                    }
                    else if (_auth.IsOnboardingDone())
                    {
                        NavigationHelper.SetRoot(ShellRouter.CreateForRole("User"));
                    }
                    else
                    {
                        GoToOnboarding();
                    }
                }
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", $"Koneksi gagal: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navigasi manual memakai NavigationPage — TIDAK memakai Shell.Current,
        /// karena halaman Login berdiri di luar Shell dan Shell.Current bisa null (crash).
        /// </summary>
        private void GoToOnboarding()
        {
            var onboardingPage = App.ServiceProvider.GetRequiredService<OnboardingPage>();
            NavigationHelper.SetRoot(new NavigationPage(onboardingPage));
        }
    }
}
