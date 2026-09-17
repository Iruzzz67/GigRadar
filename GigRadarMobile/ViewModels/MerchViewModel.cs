using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Merchandise artist (GIGRADAR_APP_SPEC.md Bab 31): kelola produk dengan
/// link pembelian eksternal (MVP — bukan marketplace penuh).
/// </summary>
public partial class MerchViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    public partial ObservableCollection<MerchItem> Items { get; set; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsFormVisible { get; set; }
    [ObservableProperty]
    public partial string FormName { get; set; } = "";
    [ObservableProperty]
    public partial string FormDescription { get; set; } = "";
    [ObservableProperty]
    public partial string FormPrice { get; set; } = "";
    [ObservableProperty]
    public partial string FormStock { get; set; } = "";
    [ObservableProperty]
    public partial string FormImageUrl { get; set; } = "";
    [ObservableProperty]
    public partial string FormExternalUrl { get; set; } = "";
    [ObservableProperty]
    public partial string FormError { get; set; } = "";
    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    public bool HasItems => Items.Count > 0;

    public MerchViewModel(ApiService api, AuthService auth)
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
            _api.SetAuthToken(_auth.GetToken());
            var items = await _api.GetMyMerchandiseAsync();
            Items = new ObservableCollection<MerchItem>(items);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", "Gagal memuat merchandise: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleForm()
    {
        IsFormVisible = !IsFormVisible;
        FormError = "";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        FormError = "";
        if (string.IsNullOrWhiteSpace(FormName))
        {
            FormError = "Nama produk wajib diisi";
            return;
        }
        if (!decimal.TryParse(FormPrice.Trim(), out var price) || price < 0)
        {
            FormError = "Harga tidak valid";
            return;
        }
        var stock = 0;
        if (!string.IsNullOrWhiteSpace(FormStock) && (!int.TryParse(FormStock.Trim(), out stock) || stock < 0))
        {
            FormError = "Stok tidak valid";
            return;
        }

        try
        {
            IsSaving = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.CreateMerchandiseAsync(
                FormName, FormDescription, FormImageUrl, price, stock, FormExternalUrl);
            if (!success)
            {
                FormError = message;
                return;
            }

            ClearForm();
            IsFormVisible = false;
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ArchiveAsync(MerchItem? item)
    {
        if (item == null) return;
        var ok = await Alerts.ConfirmAsync("Arsipkan Produk",
            $"Arsipkan \"{item.Name}\"?\nProduk tidak lagi tampil di katalog publik.");
        if (!ok) return;

        try
        {
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.ArchiveMerchandiseAsync(item.MerchandiseId);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", message);
                return;
            }
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", ex.Message);
        }
    }

    private void ClearForm()
    {
        FormName = "";
        FormDescription = "";
        FormPrice = "";
        FormStock = "";
        FormImageUrl = "";
        FormExternalUrl = "";
        FormError = "";
    }
}
