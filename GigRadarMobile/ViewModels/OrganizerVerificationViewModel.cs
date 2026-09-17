using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Halaman "Verifikasi EO" (GIGRADAR_APP_SPEC.md Bab 13–15):
/// menampilkan profil organizer, status verifikasi, jumlah portofolio,
/// dan form pengajuan (contact wajib) bila boleh mengajukan.
/// Alur lengkap: lengkapi profil → tambah portofolio → ajukan → Admin memutuskan.
/// </summary>
public partial class OrganizerVerificationViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    [NotifyPropertyChangedFor(nameof(HasRejectedReason))]
    [NotifyPropertyChangedFor(nameof(RejectedReasonDisplay))]
    [NotifyPropertyChangedFor(nameof(IsPendingReview))]
    [NotifyPropertyChangedFor(nameof(IsVerifiedBadgeVisible))]
    [NotifyPropertyChangedFor(nameof(OrganizationNameDisplay))]
    [NotifyPropertyChangedFor(nameof(DescriptionDisplay))]
    [NotifyPropertyChangedFor(nameof(LocationDisplay))]
    [NotifyPropertyChangedFor(nameof(StatusDisplayText))]
    [NotifyPropertyChangedFor(nameof(StatusDotColor))]
    [NotifyPropertyChangedFor(nameof(PortfolioCountLabel))]
    [NotifyPropertyChangedFor(nameof(HasLatest))]
    [NotifyPropertyChangedFor(nameof(LatestSubmittedAtDisplay))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial OrganizerVerificationStatus? Status { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";
    [ObservableProperty]
    public partial bool IsFormVisible { get; set; }

    partial void OnIsFormVisibleChanged(bool value) => OnPropertyChanged(nameof(ToggleFormLabel));
    [ObservableProperty]
    public partial string FormContact { get; set; } = "";
    [ObservableProperty]
    public partial string FormError { get; set; } = "";
    [ObservableProperty]
    public partial bool IsSubmitting { get; set; }

    // Properti computed untuk binding XAML — aman saat Status null.
    public bool HasStatus => Status != null;
    public bool HasRejectedReason => Status?.Latest?.HasRejectedReason == true;
    public string RejectedReasonDisplay => Status?.Latest?.RejectedReason ?? "";
    public bool IsPendingReview => Status?.VerificationStatus == "PENDING";
    public bool IsVerifiedBadgeVisible => Status?.IsVerified == true;
    public string OrganizationNameDisplay => Status?.OrganizationName ?? "";
    public string DescriptionDisplay => Status == null ? "" : "Lengkapi deskripsi di halaman Portfolio & Profil EO.";
    public string LocationDisplay => "";
    public string StatusDisplayText => Status?.StatusDisplay ?? "";
    public string StatusDotColor => Status?.StatusColor ?? "#71717A";
    public string PortfolioCountLabel => $"{Status?.PortfolioCount ?? 0} portofolio event";
    public bool HasLatest => Status?.Latest != null;
    public string LatestSubmittedAtDisplay => Status?.Latest?.SubmittedAtDisplay ?? "";
    public bool CanSubmit => Status?.CanSubmit == true;
    public string ToggleFormLabel => IsFormVisible ? "Tutup form" : "Ajukan verifikasi";

    public OrganizerVerificationViewModel(ApiService api, AuthService auth)
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

            Status = await _api.GetMyOrganizerVerificationStatusAsync();
            IsFormVisible = Status?.CanSubmit == true;
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat status verifikasi: " + ex.Message;
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
    private async Task SubmitAsync()
    {
        // Validasi client mengikuti aturan server (Bab 13 + service) — UX saja.
        FormError = "";
        var digits = FormContact.Trim().Count(char.IsDigit);
        if (digits < 9)
        {
            FormError = "Nomor telepon / kontak tidak valid (minimal 9 digit)";
            return;
        }

        try
        {
            IsSubmitting = true;
            _api.SetAuthToken(_auth.GetToken());

            var (success, message) = await _api.SubmitOrganizerVerificationAsync(FormContact);
            if (!success)
            {
                FormError = message;
                return;
            }

            IsFormVisible = false;
            FormContact = "";
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
