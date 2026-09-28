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
            Routing.RegisterRoute(nameof(EoGigsPage), typeof(EoGigsPage));
            Routing.RegisterRoute(nameof(ArtistGigsPage), typeof(ArtistGigsPage));
            Routing.RegisterRoute(nameof(ArtistVerificationPage), typeof(ArtistVerificationPage));
            Routing.RegisterRoute(nameof(OrganizerVerificationPage), typeof(OrganizerVerificationPage));
            Routing.RegisterRoute(nameof(EoPortfolioPage), typeof(EoPortfolioPage));
            Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
            Routing.RegisterRoute(nameof(FollowingPage), typeof(FollowingPage));
            Routing.RegisterRoute(nameof(CheckInPage), typeof(CheckInPage));
            Routing.RegisterRoute(nameof(AttendeesPage), typeof(AttendeesPage));
            Routing.RegisterRoute(nameof(GenresPage), typeof(GenresPage));
            Routing.RegisterRoute(nameof(InvitationsPage), typeof(InvitationsPage));
            Routing.RegisterRoute(nameof(MerchPage), typeof(MerchPage));
            Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));
            Routing.RegisterRoute(nameof(PlatformAnalyticsPage), typeof(PlatformAnalyticsPage));
        }
    }
}