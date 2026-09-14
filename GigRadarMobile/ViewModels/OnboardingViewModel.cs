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
        [ObservableProperty] private bool _hasError;
        [ObservableProperty] private string _errorMessage = "";
        [ObservableProperty] private string _selectedCountLabel = "Belum ada genre dipilih";

        public OnboardingViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadGenresAsync()
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var genres = await _api.GetGenresAsync();
                Genres = new ObservableCollection<Genre>(genres);
            }
            catch (Exception ex)
            {
                // Error inline + retry — bukan alert yang bisa terlewat.
                HasError = true;
                ErrorMessage = "Gagal memuat genre: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            if (!IsLoading)
                await LoadGenresAsync();
        }

        [RelayCommand]
        private void ToggleGenre(Genre genre)
        {
            genre.IsSelected = !genre.IsSelected;
            if (genre.IsSelected)
                SelectedGenreIds.Add(genre.GenreId);
            else
                SelectedGenreIds.Remove(genre.GenreId);

            // Counter selalu terlihat — user tahu progres tanpa menebak.
            SelectedCountLabel = SelectedGenreIds.Count == 0
                ? "Belum ada genre dipilih"
                : $"{SelectedGenreIds.Count} genre dipilih";
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
