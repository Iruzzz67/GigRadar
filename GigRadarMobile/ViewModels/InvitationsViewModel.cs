using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Halaman Undangan (GIGRADAR_APP_SPEC.md Bab 25–26):
/// - Artist: undangan masuk + accept/reject.
/// - EO: undangan terkirim + cancel, dan form undang artist ke event miliknya.
/// </summary>
public partial class InvitationsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReceived))]
    public partial ObservableCollection<InvitationItem> Received { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSent))]
    public partial ObservableCollection<InvitationItem> Sent { get; set; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsProcessing { get; set; }
    [ObservableProperty]
    public partial bool IsEo { get; set; }
    [ObservableProperty]
    public partial bool IsInviteFormVisible { get; set; }

    // Form undang (EO): event + artist dipilih lewat id sederhana
    [ObservableProperty]
    public partial string InviteEventId { get; set; } = "";
    [ObservableProperty]
    public partial string InviteArtistId { get; set; } = "";
    [ObservableProperty]
    public partial string InviteMessage { get; set; } = "";
    [ObservableProperty]
    public partial string InviteError { get; set; } = "";

    public bool HasReceived => Received.Count > 0;
    public bool HasSent => Sent.Count > 0;

    public InvitationsViewModel(ApiService api, AuthService auth)
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
            IsEo = string.Equals(_auth.GetUserRole(), "EO", StringComparison.OrdinalIgnoreCase);

            var result = await _api.GetMyInvitationsAsync();
            Received = new ObservableCollection<InvitationItem>(result?.Received ?? new());
            Sent = new ObservableCollection<InvitationItem>(result?.Sent ?? new());
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", "Gagal memuat undangan: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleInviteForm()
    {
        IsInviteFormVisible = !IsInviteFormVisible;
        InviteError = "";
    }

    [RelayCommand]
    private async Task SendInviteAsync()
    {
        InviteError = "";
        if (!int.TryParse(InviteEventId.Trim(), out var eventId) || eventId <= 0)
        {
            InviteError = "ID event tidak valid";
            return;
        }
        if (!int.TryParse(InviteArtistId.Trim(), out var artistId) || artistId <= 0)
        {
            InviteError = "ID artist tidak valid";
            return;
        }

        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.InviteArtistAsync(eventId, artistId, InviteMessage);
            if (!success)
            {
                InviteError = message;
                return;
            }

            InviteEventId = "";
            InviteArtistId = "";
            InviteMessage = "";
            IsInviteFormVisible = false;
            await Alerts.ShowAsync("Berhasil", message);
            await LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            InviteError = ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>Artist menerima undangan — masuk line-up event (Bab 25).</summary>
    [RelayCommand]
    private async Task AcceptAsync(InvitationItem? item)
    {
        if (item == null || IsProcessing) return;
        var ok = await Alerts.ConfirmAsync("Terima Undangan",
            $"Terima undangan untuk \"{item.EventName}\"?\nKamu akan masuk line-up event.");
        if (!ok) return;

        await RespondAsync(item, accept: true);
    }

    /// <summary>Artist menolak undangan.</summary>
    [RelayCommand]
    private async Task RejectAsync(InvitationItem? item)
    {
        if (item == null || IsProcessing) return;
        var ok = await Alerts.ConfirmAsync("Tolak Undangan",
            $"Tolak undangan untuk \"{item.EventName}\"?");
        if (!ok) return;

        await RespondAsync(item, accept: false);
    }

    /// <summary>EO membatalkan undangan yang masih menunggu (Bab 26 — CANCELLED).</summary>
    [RelayCommand]
    private async Task CancelAsync(InvitationItem? item)
    {
        if (item == null || IsProcessing) return;
        var ok = await Alerts.ConfirmAsync("Batalkan Undangan",
            $"Batalkan undangan untuk \"{item.ArtistName}\"?");
        if (!ok) return;

        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.CancelInvitationAsync(item.InvitationId);
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
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task RespondAsync(InvitationItem item, bool accept)
    {
        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            var (success, message) = await _api.RespondInvitationAsync(item.InvitationId, accept);
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
