using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Models;
using GigRadarMobile.Helpers;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    public partial class TicketViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        [ObservableProperty] private ObservableCollection<Ticket> _tickets = new();
        [ObservableProperty] private bool _isLoading;

        public TicketViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        [RelayCommand]
        private async Task LoadTicketsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var tickets = await _api.GetMyTicketsAsync();
                Tickets = new ObservableCollection<Ticket>(tickets);
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
