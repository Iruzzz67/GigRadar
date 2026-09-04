using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GigRadarMobile.Models;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Satu event pada halaman Kelola Tiket: berisi daftar tipe tiket miliknya
    /// dan status expand/collapse di UI.
    /// </summary>
    public partial class ManagedEventItem : ObservableObject
    {
        public GigEvent Event { get; set; } = new();

        [ObservableProperty] private bool _isExpanded;
        [ObservableProperty] private bool _isLoadingTypes;

        public ObservableCollection<EventTicketType> Types { get; set; } = new();

        public string DateLabel => Event.DateFormatted + " · " + Event.VenueName;
        public string ExpandLabel => IsExpanded ? "Tutup" : "Lihat tipe tiket";
    }
}