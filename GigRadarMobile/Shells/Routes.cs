using GigRadarMobile.Views;

namespace GigRadarMobile.Shells
{
    /// <summary>
    /// Daftar route halaman yang bisa dinavigasi lewat Shell.Current.GoToAsync
    /// (Routing.RegisterRoute bersifat global, jadi aman dipanggil dari setiap shell).
    /// </summary>
    public static class Routes
    {
        public static void Register()
        {
            Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));
            Routing.RegisterRoute(nameof(ArtistDetailPage), typeof(ArtistDetailPage));
            Routing.RegisterRoute(nameof(TicketSelectionPage), typeof(TicketSelectionPage));
            Routing.RegisterRoute(nameof(CheckoutPage), typeof(CheckoutPage));
            Routing.RegisterRoute(nameof(TicketSuccessPage), typeof(TicketSuccessPage));
            Routing.RegisterRoute(nameof(ManageTicketsPage), typeof(ManageTicketsPage));
            Routing.RegisterRoute(nameof(EoProfilePage), typeof(EoProfilePage));
            Routing.RegisterRoute(nameof(CreateEventPage), typeof(CreateEventPage));
        }
    }
}