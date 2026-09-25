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

    [ObservableProperty]
    public partial CommunityDirectory? Communities { get; set; }

    [ObservableProperty]
    public partial bool HasCommunities { get; set; }

    [ObservableProperty]
    public partial string SelectedCommunity { get; set; } = "Users";

    public bool IsUsersSelected => SelectedCommunity == "Users";
    public bool IsArtistsSelected => SelectedCommunity == "Artists";
    public bool IsEosSelected => SelectedCommunity == "EO";
    public string SelectedCommunityLabel => SelectedCommunity == "EO" ? "Event Organizer" : SelectedCommunity;

    partial void OnSelectedCommunityChanged(string value)
    {
        OnPropertyChanged(nameof(IsUsersSelected));
        OnPropertyChanged(nameof(IsArtistsSelected));
        OnPropertyChanged(nameof(IsEosSelected));
        OnPropertyChanged(nameof(SelectedCommunityLabel));
    }

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
            Communities = await _api.GetCommunityDirectoryAsync();
            HasCommunities = Communities != null;
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

    [RelayCommand]
    private void SelectCommunity(string? community)
    {
        if (!string.IsNullOrWhiteSpace(community))
            SelectedCommunity = community;
    }
}
