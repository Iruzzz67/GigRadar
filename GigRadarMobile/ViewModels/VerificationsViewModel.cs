using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Verification Center Admin (GIGRADAR_APP_SPEC.md Bab 22) — daftar pengajuan
/// verifikasi artist yang menunggu tinjauan + riwayat keputusan.
/// Approve langsung; reject wajib beralasan (diteruskan ke backend).
/// </summary>
public partial class VerificationsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPending))]
    private ObservableCollection<ArtistVerificationItem> _pending = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHistory))]
    private ObservableCollection<ArtistVerificationItem> _history = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private bool _isProcessing;

    public bool HasPending => Pending.Count > 0;
    public bool HasHistory => History.Count > 0;

    public VerificationsViewModel(ApiService api, AuthService auth)
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

            // Satu panggilan tanpa filter, lalu dipisah di client: PENDING vs keputusan.
            var all = await _api.GetArtistVerificationsAsync(status: "");
            Pending = new ObservableCollection<ArtistVerificationItem>(
                all.Where(v => v.Status == "PENDING").OrderBy(v => v.SubmittedAt));
            History = new ObservableCollection<ArtistVerificationItem>(
                all.Where(v => v.Status != "PENDING").OrderByDescending(v => v.SubmittedAt));
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat pengajuan: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApproveAsync(ArtistVerificationItem? item)
    {
        if (item == null || IsProcessing) return;

        var confirmed = await Alerts.ConfirmAsync(
            "Setujui verifikasi",
            $"Berikan badge verified pada \"{item.ArtistName}\"?");
        if (!confirmed) return;

        await ProcessAsync(() => _api.ReviewArtistVerificationAsync(item.VerificationId, approve: true));
    }

    [RelayCommand]
    private async Task RejectAsync(ArtistVerificationItem? item)
    {
        if (item == null || IsProcessing) return;

        var reason = await Alerts.PromptAsync(
            "Tolak verifikasi",
            $"Tuliskan alasan penolakan untuk \"{item.ArtistName}\" (dilihat oleh artist).",
            placeholder: "Mis. bukti tidak cukup");
        if (string.IsNullOrWhiteSpace(reason))
        {
            await Alerts.ShowAsync("Alasan wajib", "Penolakan harus disertai alasan agar artist tahu apa yang perlu diperbaiki.");
            return;
        }

        await ProcessAsync(() => _api.ReviewArtistVerificationAsync(item.VerificationId, approve: false, reason));
    }

    private async Task ProcessAsync(Func<Task<(bool Success, string Message)>> action)
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "";
            _api.SetAuthToken(_auth.GetToken());

            var (_, message) = await action();
            StatusMessage = message;
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memproses verifikasi: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
