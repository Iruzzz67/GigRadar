using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Statistik platform untuk Admin (GIGRADAR_APP_SPEC.md Bab 69 — Admin, STEP 21):
/// users, artists, EO, events, tiket, antrean verifikasi, laporan, pendapatan.
/// </summary>
public partial class PlatformAnalyticsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "";

    [ObservableProperty]
    public partial PlatformAnalytics? Stats { get; set; }

    [ObservableProperty]
    public partial bool HasData { get; set; }

    public PlatformAnalyticsViewModel(ApiService api, AuthService auth)
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

            Stats = await _api.GetPlatformAnalyticsAsync();
            HasData = Stats != null;
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat statistik platform: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
