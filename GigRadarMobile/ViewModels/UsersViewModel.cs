using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Users" (AdminShell) — daftar seluruh user platform (GET /api/users, khusus Admin).
    /// </summary>
    public partial class UsersViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private ObservableCollection<User> _users = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "";

        public UsersViewModel(ApiService api, AuthService auth)
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
                Users = new ObservableCollection<User>(await _api.GetUsersAsync());
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal memuat user: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}