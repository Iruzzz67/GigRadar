using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(GigEvent), "Event")]
    public partial class TicketSelectionViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private GigEvent? _gigEvent;
        [ObservableProperty] private ObservableCollection<EventTicketType> _ticketTypes = new();
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _hasTypes = true;
        [ObservableProperty] private string _statusMessage = "";

        public TicketSelectionViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadTypesAsync()
        {
            if (GigEvent == null) return;
            IsLoading = true;
            StatusMessage = "";

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var types = await _api.GetEventTicketTypesAsync(GigEvent.EventId);
                TicketTypes = new ObservableCollection<EventTicketType>(types);
                HasTypes = TicketTypes.Count > 0;

                if (!HasTypes)
                    StatusMessage = "Event ini belum memiliki tipe tiket. Coba lagi nanti.";
            }
            catch (Exception ex)
            {
                HasTypes = false;
                StatusMessage = "Gagal memuat tipe tiket: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SelectTypeAsync(EventTicketType type)
        {
            if (type == null || type.IsSoldOut) return;
            if (GigEvent == null) return;

            await Shell.Current.GoToAsync(nameof(CheckoutPage), new Dictionary<string, object>
            {
                { "Event", GigEvent },
                { "Type", type }
            });
        }

        [RelayCommand]
        private async Task OpenExternalLinkAsync()
        {
            if (GigEvent == null || !GigEvent.HasExternalLink) return;

            try
            {
                await Launcher.OpenAsync(GigEvent.TicketLink);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", "Gagal membuka link: " + ex.Message);
            }
        }
    }
}