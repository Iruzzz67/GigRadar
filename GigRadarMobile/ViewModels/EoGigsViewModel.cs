using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Kelola gig artist pada satu event milik EO (research §5/§6):
/// buat gig, ubah status DRAFT/OPEN/CLOSED, hapus, dan tinjau lamaran
/// (shortlist / tolak) per gig.
/// </summary>
[QueryProperty(nameof(EventId), "eventId")]
[QueryProperty(nameof(EventName), "eventName")]
public partial class EoGigsViewModel(ApiService api, AuthService auth) : ObservableObject
{
    private GigEvent? _event;

    [ObservableProperty]
    public partial string EventId { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string EventName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial ObservableCollection<GigItem> Gigs { get; set; } = new();
    [ObservableProperty]
    public partial bool HasGigs { get; set; }
    [ObservableProperty]
    public partial bool ShowEmptyState { get; set; }
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsSaving { get; set; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string GigName { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Requirements { get; set; } = string.Empty;
    [ObservableProperty]
    public partial DateTime GigDate { get; set; } = DateTime.Today;
    [ObservableProperty]
    public partial DateTime EventStartDate { get; set; } = DateTime.Today;
    [ObservableProperty]
    public partial DateTime EventEndDate { get; set; } = DateTime.Today;
    [ObservableProperty]
    public partial TimeSpan StartTime { get; set; } = new(19, 0, 0);
    [ObservableProperty]
    public partial TimeSpan EndTime { get; set; } = new(20, 0, 0);
    [ObservableProperty]
    public partial string BudgetMinText { get; set; } = "0";
    [ObservableProperty]
    public partial string BudgetMaxText { get; set; } = "0";
    [ObservableProperty]
    public partial string RequiredArtistCountText { get; set; } = "1";
    [ObservableProperty]
    public partial string SelectedStatus { get; set; } = "OPEN";

    // ── Panel lamaran gig terpilih ──
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedGig))]
    public partial GigItem? SelectedGig { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<GigApplicationItem> Applications { get; set; } = new();
    [ObservableProperty]
    public partial bool HasApplications { get; set; }
    [ObservableProperty]
    public partial bool ShowApplicationsEmpty { get; set; }
    [ObservableProperty]
    public partial bool IsLoadingApplications { get; set; }
    [ObservableProperty]
    public partial bool IsProcessingApplication { get; set; }
    [ObservableProperty]
    public partial string ApplicationsStatusMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string SelectedGigLabel { get; set; } = string.Empty;

    public List<string> StatusOptions { get; } = new() { "OPEN", "DRAFT" };

    public bool HasSelectedGig => SelectedGig != null;

    public string VenueAndGenreLabel =>
        string.Join(" · ", new[] { _event?.Venue?.Name, _event?.Genre?.Name }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!int.TryParse(EventId, out var eventId) || eventId <= 0)
        {
            StatusMessage = "Event tidak valid.";
            ShowEmptyState = false;
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;
        ShowEmptyState = false;
        try
        {
            api.SetAuthToken(auth.GetToken());
            _event = (await api.GetManagedEventsAsync()).FirstOrDefault(item => item.EventId == eventId);
            if (_event == null)
            {
                StatusMessage = "Event tidak ditemukan.";
                Gigs.Clear();
                HasGigs = false;
                ShowEmptyState = false;
                return;
            }

            EventName = _event.Name;
            OnPropertyChanged(nameof(VenueAndGenreLabel));
            GigDate = _event.StartDate.Date;
            EventStartDate = _event.StartDate.Date;
            EventEndDate = _event.EndDate.Date;
            StartTime = _event.StartDate.TimeOfDay;
            EndTime = _event.EndDate.TimeOfDay;

            await RefreshGigsAsync(eventId);
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
    private async Task CreateAsync()
    {
        if (IsSaving)
            return;

        if (_event == null || !int.TryParse(EventId, out var eventId))
        {
            await Alerts.ShowAsync("Event tidak tersedia", "Muat ulang halaman sebelum membuat gig.");
            return;
        }

        var validation = Validate();
        if (validation != null)
        {
            await Alerts.ShowAsync("Validasi", validation);
            return;
        }

        IsSaving = true;
        StatusMessage = string.Empty;
        try
        {
            api.SetAuthToken(auth.GetToken());
            var data = new CreateGigData
            {
                Name = GigName.Trim(),
                Requirements = Requirements.Trim(),
                StartAt = GigDate.Date + StartTime,
                EndAt = GetEndAt(),
                BudgetMin = ParseAmount(BudgetMinText),
                BudgetMax = ParseAmount(BudgetMaxText),
                RequiredArtistCount = int.Parse(RequiredArtistCountText, CultureInfo.InvariantCulture),
                Status = SelectedStatus
            };
            var (success, message, gig) = await api.CreateGigAsync(eventId, data);
            if (!success || gig == null)
            {
                await Alerts.ShowAsync("Gig tidak dibuat", message);
                return;
            }

            Gigs.Add(gig);
            HasGigs = true;
            ShowEmptyState = false;
            GigName = string.Empty;
            Requirements = string.Empty;
            await Alerts.ShowAsync("Gig dibuat", message);
        }
        catch (Exception ex)
        {
            StatusMessage = "Gagal membuat gig: " + ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>Buka/tutup lamaran gig (transisi DRAFT ⇄ OPEN → CLOSED divalidasi server).</summary>
    [RelayCommand]
    private async Task ToggleGigStatusAsync(GigItem? item)
    {
        if (item == null || !int.TryParse(EventId, out var eventId)) return;

        var target = item.IsOpen ? "CLOSED" : "OPEN";
        var message = item.IsOpen
            ? $"Tutup lamaran untuk \"{item.Name}\"?\nArtist tidak bisa melamar lagi sampai dibuka kembali."
            : $"Buka lamaran untuk \"{item.Name}\"?\nGig akan tampil di Discover artist.";

        var ok = await Alerts.ConfirmAsync("Ubah status gig", message);
        if (!ok) return;

        try
        {
            api.SetAuthToken(auth.GetToken());
            var (success, result) = await api.UpdateGigStatusAsync(eventId, item.GigId, target);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", result);
                return;
            }

            await Alerts.ShowAsync("Berhasil", result);
            await RefreshGigsAsync(eventId);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteGigAsync(GigItem? item)
    {
        if (item == null || !int.TryParse(EventId, out var eventId)) return;

        var ok = await Alerts.ConfirmAsync("Hapus gig",
            $"Hapus gig \"{item.Name}\" secara permanen?\nSemua lamaran artist untuk gig ini ikut terhapus.",
            accept: "Ya, Hapus", cancel: "Batal");
        if (!ok) return;

        try
        {
            api.SetAuthToken(auth.GetToken());
            var (success, message) = await api.DeleteGigAsync(eventId, item.GigId);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", message);
                return;
            }

            if (SelectedGig?.GigId == item.GigId)
                ClearSelectedGig();

            await Alerts.ShowAsync("Berhasil", message);
            await RefreshGigsAsync(eventId);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", ex.Message);
        }
    }

    /// <summary>Muat lamaran yang masuk untuk satu gig.</summary>
    [RelayCommand]
    private async Task ShowApplicationsAsync(GigItem? item)
    {
        if (item == null) return;

        SelectedGig = item;
        SelectedGigLabel = item.Name;
        ApplicationsStatusMessage = string.Empty;
        IsLoadingApplications = true;
        try
        {
            api.SetAuthToken(auth.GetToken());
            var items = await api.GetGigApplicationsAsync(item.GigId);
            Applications = new ObservableCollection<GigApplicationItem>(items);
            HasApplications = Applications.Count > 0;
            ShowApplicationsEmpty = !HasApplications;
        }
        catch (Exception ex)
        {
            Applications = new ObservableCollection<GigApplicationItem>();
            HasApplications = false;
            ShowApplicationsEmpty = false;
            ApplicationsStatusMessage = "Gagal memuat lamaran: " + ex.Message;
        }
        finally
        {
            IsLoadingApplications = false;
        }
    }

    [RelayCommand]
    private async Task ShortlistApplicationAsync(GigApplicationItem? item)
    {
        if (item == null || !item.CanShortlist || IsProcessingApplication) return;

        var ok = await Alerts.ConfirmAsync("Shortlist artist",
            $"Masukkan \"{item.ArtistName}\" ke daftar kandidat gig ini?");
        if (!ok) return;

        await DecideApplicationAsync(item, shortlist: true);
    }

    [RelayCommand]
    private async Task RejectApplicationAsync(GigApplicationItem? item)
    {
        if (item == null || !item.CanReject || IsProcessingApplication) return;

        var ok = await Alerts.ConfirmAsync("Tolak lamaran",
            $"Tolak lamaran \"{item.ArtistName}\" untuk gig ini?");
        if (!ok) return;

        await DecideApplicationAsync(item, shortlist: false);
    }

    private async Task DecideApplicationAsync(GigApplicationItem item, bool shortlist)
    {
        IsProcessingApplication = true;
        ApplicationsStatusMessage = string.Empty;
        try
        {
            api.SetAuthToken(auth.GetToken());
            var (success, message) = shortlist
                ? await api.ShortlistGigApplicationAsync(item.GigApplicationId)
                : await api.RejectGigApplicationAsync(item.GigApplicationId);
            if (!success)
            {
                await Alerts.ShowAsync("Gagal", message);
                return;
            }

            if (SelectedGig != null)
                await ShowApplicationsAsync(SelectedGig);
        }
        catch (Exception ex)
        {
            ApplicationsStatusMessage = "Gagal memproses lamaran: " + ex.Message;
        }
        finally
        {
            IsProcessingApplication = false;
        }
    }

    private async Task RefreshGigsAsync(int eventId)
    {
        Gigs = new ObservableCollection<GigItem>(await api.GetEventGigsAsync(eventId));
        HasGigs = Gigs.Count > 0;
        ShowEmptyState = !HasGigs;

        // Pertahankan panel lamaran tetap sinkron setelah gig diubah/dihapus.
        if (SelectedGig != null)
        {
            var refreshed = Gigs.FirstOrDefault(gig => gig.GigId == SelectedGig.GigId);
            if (refreshed == null)
                ClearSelectedGig();
            else
                await ShowApplicationsAsync(refreshed);
        }
    }

    private void ClearSelectedGig()
    {
        SelectedGig = null;
        SelectedGigLabel = string.Empty;
        Applications = new ObservableCollection<GigApplicationItem>();
        HasApplications = false;
        ShowApplicationsEmpty = false;
        ApplicationsStatusMessage = string.Empty;
    }

    private string? Validate()
    {
        if (string.IsNullOrWhiteSpace(GigName) || GigName.Trim().Length < 3)
            return "Nama gig wajib diisi (minimal 3 karakter).";
        if (!TryParseAmount(BudgetMinText, out var min) || min < 0)
            return "Budget minimum harus berupa angka nol atau lebih.";
        if (!TryParseAmount(BudgetMaxText, out var max) || max < min)
            return "Budget maksimum harus berupa angka dan tidak boleh di bawah budget minimum.";
        if (!int.TryParse(RequiredArtistCountText, out var count) || count is < 1 or > 20)
            return "Jumlah artist yang dibutuhkan harus antara 1 dan 20.";

        var start = GigDate.Date + StartTime;
        var end = GetEndAt();
        if (end <= start)
            return "Waktu selesai harus setelah waktu mulai.";
        if (start < _event!.StartDate || end > _event.EndDate)
            return "Jadwal gig harus berada di dalam jadwal event.";
        if (_event.VenueId == null)
            return "Event belum memiliki venue.";
        if (SelectedStatus is not ("OPEN" or "DRAFT"))
            return "Pilih status Open atau Draf.";

        return null;
    }

    private DateTime GetEndAt()
    {
        var end = GigDate.Date + EndTime;
        if (EndTime < StartTime)
            end = end.AddDays(1);
        return end;
    }

    private static decimal ParseAmount(string value) => TryParseAmount(value, out var amount) ? amount : 0;

    private static bool TryParseAmount(string value, out decimal amount)
    {
        var cleaned = (value ?? string.Empty).Replace("Rp", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".", "").Replace(",", "").Replace(" ", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }
}
