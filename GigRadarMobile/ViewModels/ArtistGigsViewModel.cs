using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Discover gig untuk artist (research §3/§5/§6): cari gig terbuka,
/// lamar, lalu pantau/ tarik lamaran sendiri.
/// </summary>
public partial class ArtistGigsViewModel : ObservableObject
{
    private const string AllGenresLabel = "Semua genre";

    private readonly ApiService _api;
    private readonly AuthService _auth;
    private readonly Dictionary<string, int> _genreIdsByName = new();

    public ArtistGigsViewModel(ApiService api, AuthService auth)
    {
        _api = api;
        _auth = auth;
        GenreNames = new ObservableCollection<string> { AllGenresLabel };
        SelectedGenreName = AllGenresLabel;
    }

    [ObservableProperty]
    public partial ObservableCollection<GigItem> Gigs { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<GigApplicationItem> MyApplications { get; set; } = new();
    /// <summary>Nama genre untuk Picker (konvensi app: daftar string, bukan objek).</summary>
    [ObservableProperty]
    public partial ObservableCollection<string> GenreNames { get; set; } = new();

    [ObservableProperty]
    public partial bool HasGigs { get; set; }
    [ObservableProperty]
    public partial bool ShowEmptyState { get; set; }
    [ObservableProperty]
    public partial bool HasApplications { get; set; }
    [ObservableProperty]
    public partial bool ShowApplicationsEmpty { get; set; }
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsProcessing { get; set; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ApplyMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SelectedGenreName { get; set; } = AllGenresLabel;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        try
        {
            _api.SetAuthToken(_auth.GetToken());

            var genres = await _api.GetGenresAsync();
            _genreIdsByName.Clear();
            var names = new List<string> { AllGenresLabel };
            foreach (var genre in genres)
            {
                _genreIdsByName[genre.Name] = genre.GenreId;
                names.Add(genre.Name);
            }
            GenreNames = new ObservableCollection<string>(names);
            if (!GenreNames.Contains(SelectedGenreName))
                SelectedGenreName = AllGenresLabel;

            await LoadGigsAsync();
            await LoadMyApplicationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat gig: " + ex.Message;
            ShowEmptyState = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        try
        {
            _api.SetAuthToken(_auth.GetToken());
            await LoadGigsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal memuat gig: " + ex.Message;
            ShowEmptyState = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAsync(GigItem? item)
    {
        if (item == null || !item.CanApply || IsProcessing) return;

        var confirmed = await Alerts.ConfirmAsync("Lamar gig",
            $"Kirim lamaran untuk \"{item.Name}\" pada \"{item.EventName}\"?\n\n" +
            $"Jadwal: {item.ScheduleLabel}\nBudget: {item.BudgetLabel}");
        if (!confirmed) return;

        IsProcessing = true;
        StatusMessage = string.Empty;
        try
        {
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.ApplyToGigAsync(item.GigId, ApplyMessage.Trim());
            if (!success)
            {
                await Alerts.ShowAsync("Lamaran gagal", message);
                return;
            }

            ApplyMessage = string.Empty;
            await Alerts.ShowAsync("Berhasil", message);
            await LoadGigsAsync();
            await LoadMyApplicationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal mengirim lamaran: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task WithdrawAsync(GigApplicationItem? item)
    {
        if (item == null || !item.CanWithdraw || IsProcessing) return;

        var confirmed = await Alerts.ConfirmAsync("Tarik lamaran",
            $"Tarik lamaran kamu untuk \"{item.TargetLabel}\"?\nKamu bisa melamar lagi selama gig masih terbuka.");
        if (!confirmed) return;

        IsProcessing = true;
        StatusMessage = string.Empty;
        try
        {
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.WithdrawGigApplicationAsync(item.GigApplicationId);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", message);
                return;
            }

            await Alerts.ShowAsync("Berhasil", message);
            await LoadGigsAsync();
            await LoadMyApplicationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal menarik lamaran: " + ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task LoadGigsAsync()
    {
        var genreId = _genreIdsByName.TryGetValue(SelectedGenreName, out var id) ? id : (int?)null;
        var items = await _api.GetOpenGigsAsync(SearchText, genreId);
        Gigs = new ObservableCollection<GigItem>(items);
        HasGigs = Gigs.Count > 0;
        ShowEmptyState = !HasGigs;
    }

    private async Task LoadMyApplicationsAsync()
    {
        MyApplications = new ObservableCollection<GigApplicationItem>(await _api.GetMyGigApplicationsAsync());
        HasApplications = MyApplications.Count > 0;
        ShowApplicationsEmpty = !HasApplications;
    }
}
