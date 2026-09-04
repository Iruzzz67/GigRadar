using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Models;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(Ticket), "Ticket")]
    public partial class TicketSuccessViewModel : ObservableObject
    {
        [ObservableProperty] private Ticket? _ticket;

        public string EventName => Ticket?.EventName ?? "";
        public string EventDate => Ticket?.EventDate ?? "";
        public string VenueName => Ticket?.VenueName ?? "";
        public string TypeName => Ticket?.TicketType ?? "";
        public string PriceFormatted => Ticket?.PriceFormatted ?? "";
        public string BuyerName => Ticket?.BuyerName ?? "";
        public string QRCode => Ticket?.QRCode ?? "";

        partial void OnTicketChanged(Ticket? value)
        {
            OnPropertyChanged(nameof(EventName));
            OnPropertyChanged(nameof(EventDate));
            OnPropertyChanged(nameof(VenueName));
            OnPropertyChanged(nameof(TypeName));
            OnPropertyChanged(nameof(PriceFormatted));
            OnPropertyChanged(nameof(BuyerName));
            OnPropertyChanged(nameof(QRCode));
        }

        [RelayCommand]
        private async Task GoToMyTicketsAsync()
        {
            // Pindah ke tab My Tickets
            await Shell.Current.GoToAsync("//tickets");
        }
    }
}