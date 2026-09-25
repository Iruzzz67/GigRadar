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
    /// Halaman admin/EO: kelola tipe tiket (Festival/Tribun/Bundling) &amp; stok per event.
    /// </summary>
    public partial class ManageTicketsViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty]
        public partial ObservableCollection<ManagedEventItem> Events { get; set; } = new();
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";

        // Form tambah/edit tipe tiket
        [ObservableProperty]
        public partial bool IsFormVisible { get; set; }
        [ObservableProperty]
        public partial string FormTitle { get; set; } = "Tambah Tipe Tiket";
        [ObservableProperty]
        public partial string FormName { get; set; } = "";
        [ObservableProperty]
        public partial string FormDescription { get; set; } = "";
        [ObservableProperty]
        public partial string FormPrice { get; set; } = "";
        [ObservableProperty]
        public partial string FormStock { get; set; } = "";
        [ObservableProperty]
        public partial int EditingTypeId { get; set; }
        [ObservableProperty]
        public partial ManagedEventItem? EditingItem { get; set; }

        public ManageTicketsViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            IsLoading = true;
            StatusMessage = "";

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var events = await _api.GetManagedEventsAsync();
                Events = new ObservableCollection<ManagedEventItem>(
                    events.Select(e => new ManagedEventItem { Event = e }));
            }
            catch (Exception ex)
            {
                StatusMessage = "Gagal memuat event: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ToggleEventAsync(ManagedEventItem item)
        {
            if (item == null) return;
            item.IsExpanded = !item.IsExpanded;
            if (item.IsExpanded)
                await LoadTypesAsync(item);
        }

        private async Task LoadTypesAsync(ManagedEventItem item)
        {
            item.IsLoadingTypes = true;
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var types = await _api.GetEventTicketTypesAsync(item.Event.EventId);
                item.Types.Clear();
                foreach (var t in types)
                    item.Types.Add(t);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gagal memuat tipe tiket untuk \"{item.Event.Name}\": {ex.Message}";
            }
            finally
            {
                item.IsLoadingTypes = false;
            }
        }

        [RelayCommand]
        private void ShowAddType(ManagedEventItem item)
        {
            if (item == null) return;
            EditingItem = item;
            EditingTypeId = 0;
            FormTitle = "Tambah Tipe Tiket";
            FormName = "";
            FormDescription = "";
            FormPrice = "";
            FormStock = "";
            IsFormVisible = true;
        }

        [RelayCommand]
        private void EditType(EventTicketType type)
        {
            if (type == null) return;
            var item = Events.FirstOrDefault(e => e.Types.Contains(type));
            if (item == null) return;

            EditingItem = item;
            EditingTypeId = type.EventTicketTypeId;
            FormTitle = $"Edit: {type.Name}";
            FormName = type.Name;
            FormDescription = type.Description;
            FormPrice = type.Price.ToString("0.##");
            FormStock = type.Stock.ToString();
            IsFormVisible = true;
        }

        [RelayCommand]
        private void CancelForm()
        {
            IsFormVisible = false;
            EditingItem = null;
            EditingTypeId = 0;
        }

        [RelayCommand]
        private async Task SaveTypeAsync()
        {
            if (EditingItem == null) return;

            if (string.IsNullOrWhiteSpace(FormName))
            {
                await Alerts.ShowAsync("Validasi", "Nama tipe tiket wajib diisi");
                return;
            }

            if (!decimal.TryParse(CleanNumber(FormPrice), out var price) || price < 0)
            {
                await Alerts.ShowAsync("Validasi", "Harga tidak valid (angka, minimal 0)");
                return;
            }

            if (!int.TryParse(CleanNumber(FormStock), out var stock) || stock < 0)
            {
                await Alerts.ShowAsync("Validasi", "Stok tidak valid (angka, minimal 0)");
                return;
            }

            var sortOrder = 0; // otomatis dihitung server (urut terakhir)

            IsFormVisible = false;
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = EditingTypeId == 0
                    ? await _api.CreateTicketTypeAsync(
                        EditingItem.Event.EventId, FormName.Trim(), FormDescription.Trim(),
                        price, stock, sortOrder)
                    : await _api.UpdateTicketTypeAsync(
                        EditingTypeId, FormName.Trim(), FormDescription.Trim(),
                        price, stock, sortOrder);

                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    IsFormVisible = true;
                    return;
                }

                await LoadTypesAsync(EditingItem);
                EditingItem = null;
                EditingTypeId = 0;
                await Alerts.ShowAsync("Berhasil", message);
            }
            catch (Exception ex)
            {
                IsFormVisible = true;
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteTypeAsync(EventTicketType type)
        {
            if (type == null) return;

            var ok = await Alerts.ConfirmAsync(
                "Hapus tipe tiket",
                $"Hapus \"{type.Name}\" ({type.PriceFormatted})? Stok yang tersisa ikut terhapus.");
            if (!ok) return;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = await _api.DeleteTicketTypeAsync(type.EventTicketTypeId);
                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                var item = Events.FirstOrDefault(e => e.Types.Contains(type));
                if (item != null)
                    await LoadTypesAsync(item);

                await Alerts.ShowAsync("Berhasil", message);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        // ── Check-in & peserta (§14) ──────────────────────

        /// <summary>Buka halaman pemindai QR untuk event yang dipilih.</summary>
        [RelayCommand]
        private async Task OpenCheckInAsync(ManagedEventItem? item)
        {
            if (item == null) return;

            try
            {
                await Shell.Current.GoToAsync(nameof(CheckInPage),
                    new Dictionary<string, object> { { "Event", item.Event } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>Buka daftar peserta event yang dipilih.</summary>
        [RelayCommand]
        private async Task OpenAttendeesAsync(ManagedEventItem? item)
        {
            if (item == null) return;

            try
            {
                await Shell.Current.GoToAsync(nameof(AttendeesPage),
                    new Dictionary<string, object> { { "Event", item.Event } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        private static string CleanNumber(string input)
        {
            return (input ?? "")
                .Replace("Rp", "")
                .Replace("rp", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "")
                .Trim();
        }
    }
}