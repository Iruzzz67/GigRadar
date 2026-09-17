using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Halaman "Portfolio EO" (GIGRADAR_APP_SPEC.md Bab 12, 14):
/// kelola profil organizer (nama organisasi, deskripsi, lokasi, web, medsos,
/// tahun berdiri) + portofolio event sebelumnya sebagai bukti verifikasi.
/// </summary>
public partial class EoPortfolioViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPortfolio))]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    public partial OrganizerProfile? Profile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPortfolio))]
    public partial ObservableCollection<PortfolioItem> Portfolio { get; set; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";
    [ObservableProperty]
    public partial bool IsVerifiedBadgeVisible { get; set; }

    // Form edit profil
    [ObservableProperty]
    public partial bool IsEditing { get; set; }
    [ObservableProperty]
    public partial string EditOrganizationName { get; set; } = "";
    [ObservableProperty]
    public partial string EditDescription { get; set; } = "";
    [ObservableProperty]
    public partial string EditLocation { get; set; } = "";
    [ObservableProperty]
    public partial string EditContact { get; set; } = "";
    [ObservableProperty]
    public partial string EditWebsiteUrl { get; set; } = "";
    [ObservableProperty]
    public partial string EditSocialLinks { get; set; } = "";
    [ObservableProperty]
    public partial string EditFoundedYear { get; set; } = "";
    [ObservableProperty]
    public partial string EditError { get; set; } = "";
    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    // Form tambah portofolio
    [ObservableProperty]
    public partial bool IsAddPortfolioVisible { get; set; }
    [ObservableProperty]
    public partial string FormEventName { get; set; } = "";
    [ObservableProperty]
    public partial string FormEventDate { get; set; } = "";
    [ObservableProperty]
    public partial string FormVenue { get; set; } = "";
    [ObservableProperty]
    public partial string FormDescription { get; set; } = "";
    [ObservableProperty]
    public partial string FormPhotosUrl { get; set; } = "";
    [ObservableProperty]
    public partial string FormDocumentationLink { get; set; } = "";
    [ObservableProperty]
    public partial string FormError { get; set; } = "";
    [ObservableProperty]
    public partial bool IsAddingPortfolio { get; set; }

    public bool HasPortfolio => Portfolio.Count > 0;
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);
    public string ToggleEditLabel => IsEditing ? "Tutup form profil" : "Edit profil";

    public EoPortfolioViewModel(ApiService api, AuthService auth)
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

            var result = await _api.GetOrganizerMeAsync();
            if (result != null)
            {
                Profile = result.Organizer;
                Portfolio = new ObservableCollection<PortfolioItem>(result.Portfolio);
                IsVerifiedBadgeVisible = result.Organizer.IsVerified;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat profil: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
        EditError = "";
        if (IsEditing && Profile != null)
        {
            EditOrganizationName = Profile.OrganizationName;
            EditDescription = Profile.Description;
            EditLocation = Profile.Location;
            EditContact = Profile.Contact;
            EditWebsiteUrl = Profile.WebsiteUrl;
            EditSocialLinks = Profile.SocialLinks;
            EditFoundedYear = Profile.FoundedYear?.ToString() ?? "";
        }
        OnPropertyChanged(nameof(ToggleEditLabel));
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        EditError = "";
        if (string.IsNullOrWhiteSpace(EditOrganizationName))
        {
            EditError = "Nama organisasi wajib diisi";
            return;
        }

        int? foundedYear = null;
        if (!string.IsNullOrWhiteSpace(EditFoundedYear))
        {
            if (!int.TryParse(EditFoundedYear.Trim(), out var year) || year < 1900 || year > DateTime.UtcNow.Year)
            {
                EditError = $"Tahun berdiri harus angka antara 1900–{DateTime.UtcNow.Year}";
                return;
            }
            foundedYear = year;
        }

        try
        {
            IsSaving = true;
            _api.SetAuthToken(_auth.GetToken());

            var (success, message) = await _api.UpdateOrganizerProfileAsync(
                EditOrganizationName, EditDescription, EditLocation, EditContact,
                EditWebsiteUrl, EditSocialLinks, foundedYear);
            if (!success)
            {
                EditError = message;
                return;
            }

            IsEditing = false;
            OnPropertyChanged(nameof(ToggleEditLabel));
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            EditError = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void ToggleAddPortfolio()
    {
        IsAddPortfolioVisible = !IsAddPortfolioVisible;
        FormError = "";
    }

    [RelayCommand]
    private async Task AddPortfolioAsync()
    {
        FormError = "";
        if (string.IsNullOrWhiteSpace(FormEventName))
        {
            FormError = "Nama event wajib diisi";
            return;
        }
        if (string.IsNullOrWhiteSpace(FormDescription) && string.IsNullOrWhiteSpace(FormDocumentationLink)
            && string.IsNullOrWhiteSpace(FormPhotosUrl))
        {
            FormError = "Sertakan deskripsi, foto, atau link dokumentasi sebagai bukti";
            return;
        }

        DateTime? eventDate = null;
        if (!string.IsNullOrWhiteSpace(FormEventDate))
        {
            if (!DateTime.TryParse(FormEventDate.Trim(), out var parsed))
            {
                FormError = "Format tanggal tidak valid (contoh: 2025-08-17)";
                return;
            }
            eventDate = parsed;
        }

        try
        {
            IsAddingPortfolio = true;
            _api.SetAuthToken(_auth.GetToken());

            var (success, message) = await _api.AddPortfolioItemAsync(
                FormEventName, eventDate, FormVenue, FormDescription, FormPhotosUrl, FormDocumentationLink);
            if (!success)
            {
                FormError = message;
                return;
            }

            ClearPortfolioForm();
            IsAddPortfolioVisible = false;
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsAddingPortfolio = false;
        }
    }

    [RelayCommand]
    private async Task DeletePortfolioAsync(PortfolioItem? item)
    {
        if (item == null) return;

        var ok = await Alerts.ConfirmAsync("Hapus Portofolio",
            $"Hapus \"{item.EventName}\" dari portofolio?", accept: "Ya, Hapus");
        if (!ok) return;

        try
        {
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.DeletePortfolioItemAsync(item.PortfolioItemId);
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

    private void ClearPortfolioForm()
    {
        FormEventName = "";
        FormEventDate = "";
        FormVenue = "";
        FormDescription = "";
        FormPhotosUrl = "";
        FormDocumentationLink = "";
        FormError = "";
    }
}
