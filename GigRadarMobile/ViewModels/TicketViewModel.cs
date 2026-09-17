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

        [ObservableProperty]
        public partial ObservableCollection<Ticket> Tickets { get; set; } = new();
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }
        [ObservableProperty]
        public partial bool HasError { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = "";
        [ObservableProperty]
        public partial bool HasTickets { get; set; }

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
            HasError = false;
            ErrorMessage = "";

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var tickets = await _api.GetMyTicketsAsync();
                Tickets = new ObservableCollection<Ticket>(tickets);
                HasTickets = tickets.Count > 0;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Gagal memuat tiket: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }
    }
}
