using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Halaman kelola data master Genre (§4.4 Admin: "Mengelola genre").
    /// Genre dipakai event &amp; preferensi user, jadi penghapusan yang masih terpakai
    /// akan ditolak server dengan penjelasan.
    /// </summary>
    public partial class AdminGenresViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty]
        public partial ObservableCollection<Genre> Genres { get; set; } = new();
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }
        [ObservableProperty]
        public partial bool HasError { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasGenres { get; set; }

        // Form tambah/ubah
        [ObservableProperty]
        public partial bool IsFormVisible { get; set; }
        [ObservableProperty]
        public partial string FormTitle { get; set; } = "Tambah genre";
        [ObservableProperty]
        public partial string FormName { get; set; } = "";
        [ObservableProperty]
        public partial string FormIcon { get; set; } = "";
        [ObservableProperty]
        public partial int EditingGenreId { get; set; }

        public AdminGenresViewModel(ApiService api, AuthService auth)
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
                HasError = false;
                ErrorMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var genres = await _api.GetGenresAsync();
                Genres = new ObservableCollection<Genre>(genres);
                HasGenres = Genres.Count > 0;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Gagal memuat genre: " + ex.Message;
                Genres = new ObservableCollection<Genre>();
                HasGenres = false;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void ShowCreateForm()
        {
            EditingGenreId = 0;
            FormTitle = "Tambah genre";
            FormName = "";
            FormIcon = "";
            StatusMessage = "";
            IsFormVisible = true;
        }

        [RelayCommand]
        private void EditGenre(Genre? genre)
        {
            if (genre == null) return;

            EditingGenreId = genre.GenreId;
            FormTitle = $"Ubah: {genre.Name}";
            FormName = genre.Name;
            FormIcon = genre.Icon;
            StatusMessage = "";
            IsFormVisible = true;
        }

        [RelayCommand]
        private void CancelForm()
        {
            IsFormVisible = false;
            EditingGenreId = 0;
        }

        [RelayCommand]
        private async Task SaveGenreAsync()
        {
            if (string.IsNullOrWhiteSpace(FormName))
            {
                await Alerts.ShowAsync("Validasi", "Nama genre wajib diisi");
                return;
            }

            IsFormVisible = false;
            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = EditingGenreId == 0
                    ? await _api.CreateGenreAsync(FormName.Trim(), FormIcon?.Trim() ?? "")
                    : await _api.UpdateGenreAsync(EditingGenreId, FormName.Trim(), FormIcon?.Trim() ?? "");

                if (!success)
                {
                    StatusMessage = message;
                    IsFormVisible = true;      // isian dipertahankan saat gagal
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                StatusMessage = message;
                EditingGenreId = 0;
                await LoadAsync();
                await Alerts.ShowAsync("Berhasil", message);
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal menyimpan genre: " + ex.Message;
                IsFormVisible = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>Hapus genre — dikonfirmasi dulu; server menolak bila masih dipakai (§23).</summary>
        [RelayCommand]
        private async Task DeleteGenreAsync(Genre? genre)
        {
            if (genre == null) return;

            var ok = await Alerts.ConfirmAsync(
                "Hapus genre",
                $"Hapus genre \"{genre.Name}\"? Genre yang masih dipakai event tidak bisa dihapus.",
                accept: "Hapus",
                cancel: "Batal");
            if (!ok) return;

            try
            {
                IsLoading = true;
                StatusMessage = "";
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = await _api.DeleteGenreAsync(genre.GenreId);
                StatusMessage = message;

                if (success) await LoadAsync();
                else await Alerts.ShowAsync("Tidak bisa dihapus", message);
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal menghapus genre: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
