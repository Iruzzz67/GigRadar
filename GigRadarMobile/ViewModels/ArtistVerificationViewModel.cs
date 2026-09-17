using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Halaman "Verifikasi Artist" (GIGRADAR_APP_SPEC.md Bab 7–8):
/// menampilkan status verifikasi + riwayat pengajuan terakhir, dan form pengajuan
/// bila boleh mengajukan (NOT_SUBMITTED / REJECTED). Keputusan selalu dari Admin.
/// </summary>
public partial class ArtistVerificationViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    [NotifyPropertyChangedFor(nameof(HasRejectedReason))]
    [NotifyPropertyChangedFor(nameof(RejectedReasonDisplay))]
    [NotifyPropertyChangedFor(nameof(IsPendingReview))]
    [NotifyPropertyChangedFor(nameof(IsVerifiedBadgeVisible))]
    [NotifyPropertyChangedFor(nameof(ArtistNameDisplay))]
    [NotifyPropertyChangedFor(nameof(StatusDisplayText))]
    [NotifyPropertyChangedFor(nameof(StatusDotColor))]
    [NotifyPropertyChangedFor(nameof(HasLatest))]
    [NotifyPropertyChangedFor(nameof(LatestSubmittedAtDisplay))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial ArtistVerificationStatus? Status { get; set; }
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";

    // Properti computed untuk binding XAML — aman saat Status null (binding gagal = elemen
    // tak pernah tampil, tidak seperti binding nested yang bisa tertinggal visible).
    public bool HasStatus => Status != null;
    public bool HasRejectedReason => Status?.Latest?.HasRejectedReason == true;
    public string RejectedReasonDisplay => Status?.Latest?.RejectedReason ?? "";
    public bool IsPendingReview => Status?.VerificationStatus == "PENDING";
    public bool IsVerifiedBadgeVisible => Status?.IsVerified == true;

    // Nilai binding datar untuk card status (hindari binding nested saat Status null).
    public string ArtistNameDisplay => Status?.Name ?? "";
    public string StatusDisplayText => Status?.StatusDisplay ?? "";
    public string StatusDotColor => Status?.StatusColor ?? "#71717A";
    public bool HasLatest => Status?.Latest != null;
    public string LatestSubmittedAtDisplay => Status?.Latest?.SubmittedAtDisplay ?? "";
    public bool CanSubmit => Status?.CanSubmit == true;
    public string ToggleFormLabel => IsFormVisible ? "Tutup form" : "Ajukan verifikasi";

    // Status form
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleFormLabel))]
    public partial bool IsFormVisible { get; set; }
    [ObservableProperty]
    public partial string FormLegalName { get; set; } = "";
    [ObservableProperty]
    public partial string FormPhone { get; set; } = "";
    [ObservableProperty]
    public partial string FormSpotifyUrl { get; set; } = "";
    [ObservableProperty]
    public partial string FormSupportingInfo { get; set; } = "";
    [ObservableProperty]
    public partial string FormError { get; set; } = "";
    [ObservableProperty]
    public partial bool IsSubmitting { get; set; }

    public ArtistVerificationViewModel(ApiService api, AuthService auth)
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

            Status = await _api.GetMyVerificationStatusAsync();
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
        // Validasi client mengikuti aturan server (Bab 7 + service) — UX saja,
        // keputusan akhir tetap dari backend.
        FormError = "";
        if (string.IsNullOrWhiteSpace(FormLegalName) || FormLegalName.Trim().Length < 3)
        {
            FormError = "Nama legal / perwakilan wajib diisi (min. 3 karakter)";
            return;
        }
        var digits = FormPhone.Trim().Where(char.IsDigit).Count();
        if (digits < 9)
        {
            FormError = "Nomor telepon tidak valid (minimal 9 digit)";
            return;
        }
        if (string.IsNullOrWhiteSpace(FormSpotifyUrl) && string.IsNullOrWhiteSpace(FormSupportingInfo))
        {
            FormError = "Sertakan link Spotify atau informasi pendukung sebagai bukti";
            return;
        }

        try
        {
            IsSubmitting = true;
            _api.SetAuthToken(_auth.GetToken());

            var (success, message) = await _api.SubmitVerificationAsync(
                FormLegalName, FormPhone, FormSpotifyUrl, FormSupportingInfo);

            if (!success)
            {
                FormError = message;
                return;
            }

            IsFormVisible = false;
            ClearForm();
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

    private void ClearForm()
    {
        FormLegalName = "";
        FormPhone = "";
        FormSpotifyUrl = "";
        FormSupportingInfo = "";
        FormError = "";
    }
}
