using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private ObservableCollection<Genre> _genres = new();
        [ObservableProperty] private ObservableCollection<int> _selectedGenreIds = new();
        [ObservableProperty] private bool _isLoading;

        public OnboardingViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadGenresAsync()
        {
            IsLoading = true;
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var genres = await _api.GetGenresAsync();
                Genres = new ObservableCollection<Genre>(genres);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleGenre(Genre genre)
        {
            if (SelectedGenreIds.Contains(genre.GenreId))
                SelectedGenreIds.Remove(genre.GenreId);
            else
                SelectedGenreIds.Add(genre.GenreId);
        }

        [RelayCommand]
        private async Task FinishOnboardingAsync()
        {
            if (SelectedGenreIds.Count == 0)
            {
                await Alerts.ShowAsync("Info", "Pilih minimal 1 genre favorit");
                return;
            }

            try
            {
                _api.SetAuthToken(_auth.GetToken());

                // Simpan preferensi genre ke server
                var saved = await _api.UpdatePreferencesAsync(SelectedGenreIds.ToList());
                if (!saved)
                {
                    await Alerts.ShowAsync("Error", "Gagal menyimpan preferensi. Coba lagi.");
                    return;
                }

                _auth.SaveOnboardingDone();
                NavigationHelper.SetRoot(ShellRouter.CreateForRole("User"));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }
    }
}
