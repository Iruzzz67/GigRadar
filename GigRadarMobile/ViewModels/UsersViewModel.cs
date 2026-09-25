using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Users" (AdminShell) — daftar seluruh user platform (GET /api/users, khusus
    /// Admin), kelola permohonan role Artist/EO (§25), dan CRUD akun (§4.4/§19/§23):
    /// buat, ubah, nonaktifkan/aktifkan, serta hapus.
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

        // ── Form akun (buat/ubah) ─────────────────────────
        [ObservableProperty]
        public partial bool IsFormVisible { get; set; }
        [ObservableProperty]
        public partial string FormTitle { get; set; } = "Buat akun";
        [ObservableProperty]
        public partial string FormName { get; set; } = "";
        [ObservableProperty]
        public partial string FormEmail { get; set; } = "";
        [ObservableProperty]
        public partial string FormPassword { get; set; } = "";
        [ObservableProperty]
        public partial string FormRole { get; set; } = "User";
        [ObservableProperty]
        public partial string FormCity { get; set; } = "";
        [ObservableProperty]
        public partial int EditingUserId { get; set; }

        /// <summary>Role yang boleh dibuat/diatur Admin (§4).</summary>
        public List<string> RoleOptions { get; } = new() { "User", "Artist", "EO", "Admin" };

        /// <summary>Petunjuk password menyesuaikan mode buat/ubah akun.</summary>
        public string PasswordHint => EditingUserId == 0
            ? "Password awal untuk akun ini."
            : "Biarkan kosong bila password tidak diubah.";

        public UsersViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        partial void OnEditingUserIdChanged(int value) => OnPropertyChanged(nameof(PasswordHint));

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

        // ── CRUD akun (§4.4/§23) ──────────────────────────

        [RelayCommand]
        private void ShowCreateForm()
        {
            EditingUserId = 0;
            FormTitle = "Buat akun baru";
            FormName = "";
            FormEmail = "";
            FormPassword = "";
            FormRole = "User";
            FormCity = "";
            StatusMessage = "";
            IsFormVisible = true;
        }

        [RelayCommand]
        private void EditAccount(User? user)
        {
            if (user == null) return;

            EditingUserId = user.UserId;
            FormTitle = $"Ubah: {user.Name}";
            FormName = user.Name;
            FormEmail = user.Email;
            FormPassword = "";      // kosong = password tidak diubah
            FormRole = user.Role;
            FormCity = user.City;
            StatusMessage = "";
            IsFormVisible = true;
        }

        [RelayCommand]
        private void CancelForm()
        {
            IsFormVisible = false;
            EditingUserId = 0;
        }

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            // Validasi cepat di client; aturan sebenarnya tetap milik server.
            if (string.IsNullOrWhiteSpace(FormName) || FormName.Trim().Length < 3)
            {
                await Alerts.ShowAsync("Validasi", "Nama wajib diisi (minimal 3 karakter)");
                return;
            }

            if (string.IsNullOrWhiteSpace(FormEmail))
            {
                await Alerts.ShowAsync("Validasi", "Email wajib diisi");
                return;
            }

            if (EditingUserId == 0 && string.IsNullOrWhiteSpace(FormPassword))
            {
                await Alerts.ShowAsync("Validasi", "Password awal wajib diisi untuk akun baru");
                return;
            }

            // Password null = pertahankan yang lama (server yang memutuskan).
            var password = string.IsNullOrWhiteSpace(FormPassword) ? null : FormPassword;

            IsFormVisible = false;
            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = EditingUserId == 0
                    ? await _api.CreateAccountAsync(FormName.Trim(), FormEmail.Trim(), password!,
                        FormRole, FormCity.Trim())
                    : await _api.UpdateAccountAsync(EditingUserId, FormName.Trim(), FormEmail.Trim(),
                        password, FormRole, FormCity.Trim());

                if (!success)
                {
                    StatusMessage = message;
                    IsFormVisible = true;       // isian tetap ada saat gagal
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                StatusMessage = message;
                EditingUserId = 0;
                await LoadAsync();
                await Alerts.ShowAsync("Berhasil", message);
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal menyimpan akun: " + ex.Message;
                IsFormVisible = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>Nonaktifkan/aktifkan akun — dikonfirmasi karena mengubah akses user (§23).</summary>
        [RelayCommand]
        private async Task ToggleSuspendAsync(User? user)
        {
            if (user == null) return;

            var suspended = !user.IsSuspended;
            var ok = await Alerts.ConfirmAsync(
                suspended ? "Nonaktifkan akun" : "Aktifkan akun",
                suspended
                    ? $"Nonaktifkan {user.Name}? Akun tidak bisa login, tapi datanya tetap tersimpan."
                    : $"Aktifkan kembali {user.Name}? Mereka bisa login lagi.",
                accept: suspended ? "Nonaktifkan" : "Aktifkan",
                cancel: "Batal");
            if (!ok) return;

            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = await _api.SetAccountSuspendedAsync(user.UserId, suspended);
                StatusMessage = message;

                if (success) await LoadAsync();
                else await Alerts.ShowAsync("Gagal", message);
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal mengubah status akun: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>Hapus akun — operasi destruktif, wajib konfirmasi (§23).</summary>
        [RelayCommand]
        private async Task DeleteAccountAsync(User? user)
        {
            if (user == null) return;

            var ok = await Alerts.ConfirmAsync(
                "Hapus akun",
                $"Hapus akun {user.Name} ({user.Email})? Tindakan ini tidak bisa dibatalkan.",
                accept: "Hapus",
                cancel: "Batal");
            if (!ok) return;

            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = await _api.DeleteAccountAsync(user.UserId);
                StatusMessage = message;

                if (success) await LoadAsync();
                else await Alerts.ShowAsync("Gagal", message);
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal menghapus akun: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>Buka halaman kelola data master genre (§4.4).</summary>
        [RelayCommand]
        private async Task GoToGenresAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(GenresPage));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }
    }
}