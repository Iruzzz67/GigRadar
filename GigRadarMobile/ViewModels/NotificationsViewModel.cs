using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels;

/// <summary>
/// Halaman Notifikasi (GIGRADAR_APP_SPEC.md Bab 36): daftar notifikasi user,
/// badge belum dibaca, tandai dibaca satu/semua.
/// </summary>
public partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly AuthService _auth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotifications))]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    public partial ObservableCollection<NotificationItem> Items { get; set; } = new();

    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    public bool HasNotifications => Items.Count > 0;
    public bool HasUnread => Items.Any(i => i.IsUnread);

    public NotificationsViewModel(ApiService api, AuthService auth)
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
            var items = await _api.GetNotificationsAsync();
            Items = new ObservableCollection<NotificationItem>(items);
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", "Gagal memuat notifikasi: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkReadAsync(NotificationItem? item)
    {
        if (item == null || item.IsRead) return;
        try
        {
            _api.SetAuthToken(_auth.GetToken());
            await _api.MarkNotificationReadAsync(item.NotificationId);
            item.IsRead = true;
            OnPropertyChanged(nameof(HasUnread));
        }
        catch (Exception ex)
        {
            await Alerts.ShowAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task MarkAllReadAsync()
    {
        try
        {
            IsProcessing = true;
            _api.SetAuthToken(_auth.GetToken());
            await _api.MarkAllNotificationsReadAsync();
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
