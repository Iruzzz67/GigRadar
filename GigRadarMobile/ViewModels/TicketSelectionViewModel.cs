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

        [ObservableProperty]
        public partial GigEvent? GigEvent { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<EventTicketType> TicketTypes { get; set; } = new();
        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool HasTypes { get; set; } = true;
        [ObservableProperty]
        public partial bool HasLoadError { get; set; }
        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "";

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
            HasLoadError = false;
            HasTypes = true; // sembunyikan pesan kosong/error selama memuat

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var types = await _api.GetEventTicketTypesAsync(GigEvent.EventId);
                TicketTypes = new ObservableCollection<EventTicketType>(types);
                HasTypes = TicketTypes.Count > 0;

                if (!HasTypes)
                {
                    HasLoadError = false;
                    StatusMessage = GigEvent.HasExternalLink
                        ? "Tiket event ini dijual melalui situs mitra resmi — gunakan tombol di bawah untuk membeli."
                        : "EO event ini belum menerbitkan tipe tiket.\n\nKamu EO-nya? Buka tab Tickets → pilih event ini → \"Tambah Tipe Tiket\" (mis. Festival/Tribun) agar pembeli bisa membeli.";
                }
            }
            catch (Exception ex)
            {
                HasTypes = false;
                HasLoadError = true;
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