using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Laporan &amp; moderasi (GIGRADAR_APP_SPEC.md Bab 37, 49):
/// - Semua role bisa membuat laporan (event/post/comment/artist/EO/merch).
/// - Admin melihat antrean laporan + resolve/dismiss dengan catatan moderasi.
/// </summary>
public partial class ReportsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReports))]
    private ObservableCollection<ReportItem> _reports = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private bool _isAdmin;

    // Form laporan baru
    [ObservableProperty] private string _targetType = "EVENT";
    [ObservableProperty] private string _targetId = "";
    [ObservableProperty] private string _category = "SPAM";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _formError = "";

    public bool HasReports => Reports.Count > 0;
    public List<string> TargetTypes { get; } = ["EVENT", "POST", "COMMENT", "ARTIST", "EO", "MERCHANDISE"];
    public List<string> Categories { get; } =
        ["SPAM", "IMPERSONATION", "FRAUD", "INAPPROPRIATE_CONTENT", "MISLEADING_INFORMATION", "OTHER"];

    public ReportsViewModel(ApiService api, AuthService auth)
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
            IsAdmin = string.Equals(_auth.GetUserRole(), "Admin", StringComparison.OrdinalIgnoreCase);

            if (IsAdmin)
            {
                var reports = await _api.GetReportsAsync("OPEN");
                Reports = new ObservableCollection<ReportItem>(reports);
            }
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", "Gagal memuat laporan: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        FormError = "";
        if (!int.TryParse(TargetId?.Trim(), out var targetId) || targetId <= 0)
        {
            FormError = "ID target tidak valid";
            return;
        }
        if (string.IsNullOrWhiteSpace(Description))
        {
            FormError = "Deskripsi laporan wajib diisi";
            return;
        }

        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.CreateReportAsync(TargetType, targetId, Category, Description);
            if (!success)
            {
                FormError = message;
                return;
            }

            Description = "";
            TargetId = "";
            await Alerts.ShowAsync("Terima kasih", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            FormError = ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ResolveAsync(ReportItem? item)
    {
        if (item == null || IsProcessing) return;
        var note = await Alerts.PromptAsync("Selesaikan Laporan",
            $"Catatan moderasi untuk {item.TargetLabel} (dilihat Admin):",
            placeholder: "Mis. konten dihapus, akun diberi peringatan");
        if (note == null) return;

        await DecideAsync(item, resolve: true, note);
    }

    [RelayCommand]
    private async Task DismissAsync(ReportItem? item)
    {
        if (item == null || IsProcessing) return;
        var ok = await Alerts.ConfirmAsync("Tolak Laporan",
            $"Tolak laporan {item.TargetLabel} tanpa tindakan?");
        if (!ok) return;

        await DecideAsync(item, resolve: false, note: "");
    }

    private async Task DecideAsync(ReportItem item, bool resolve, string note)
    {
        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.DecideReportAsync(item.ReportId, resolve, note);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", message);
                return;
            }
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", ex.Message);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
