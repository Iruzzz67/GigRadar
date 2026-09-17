using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Users" (AdminShell) — daftar seluruh user platform (GET /api/users, khusus Admin)
    /// + kelola permohonan role Artist/EO (§25): setujui atau tolak.
    /// </summary>
    public partial class UsersViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty]
        public partial ObservableCollection<User> Users { get; set; } = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasRoleRequests))]
        public partial ObservableCollection<RoleRequestItem> RoleRequests { get; set; } = new();

        /// <summary>True bila ada permohonan role yang menunggu verifikasi.</summary>
        public bool HasRoleRequests => RoleRequests.Count > 0;
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";

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
                RoleRequests = new ObservableCollection<RoleRequestItem>(
                    await _api.GetRoleRequestsAsync("Pending"));
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

        [RelayCommand]
        private async Task ApproveAsync(RoleRequestItem? request)
        {
            if (request == null) return;
            await ProcessAsync(() => _api.ApproveRoleRequestAsync(request.RequestId));
        }

        [RelayCommand]
        private async Task RejectAsync(RoleRequestItem? request)
        {
            if (request == null) return;
            await ProcessAsync(() => _api.RejectRoleRequestAsync(request.RequestId));
        }

        private async Task ProcessAsync(Func<Task<(bool Success, string Message)>> action)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());
                var (_, message) = await action();
                StatusMessage = message;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal memproses permohonan: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}