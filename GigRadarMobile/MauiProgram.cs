using GigRadarMobile.Helpers;
using GigRadarMobile.Services;
using GigRadarMobile.ViewModels;
using GigRadarMobile.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui.Controls;

// Force XamlC di semua konfigurasi (termasuk Debug): tanpa ini XAML di-parsing
// saat runtime dan error seperti property yang tidak ada (mis. Padding di Entry)
// baru meledak sebagai XamlParseException / force close saat halaman dibuka.
// Dengan XamlC, error tersebut tertangkap saat build.
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace GigRadarMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // ZXing: QR generator (semua platform) + kamera scanner check-in EO
            // (Android/iOS/MacCatalyst — di Windows view scanner jadi input manual).
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                // Display: Space Grotesk. Body: Inter.
                fonts.AddFont("SpaceGrotesk-Regular.ttf", "SpaceGrotesk");
                fonts.AddFont("SpaceGrotesk-Medium.ttf", "SpaceGroteskMedium");
                fonts.AddFont("SpaceGrotesk-Bold.ttf", "SpaceGroteskBold");
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

        // Services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<LocationService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<ExploreViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<EventDetailViewModel>();
        builder.Services.AddTransient<RadarViewModel>();
        builder.Services.AddTransient<TicketViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<TicketSelectionViewModel>();
        builder.Services.AddTransient<CheckoutViewModel>();
        builder.Services.AddTransient<TicketSuccessViewModel>();
        builder.Services.AddTransient<ArtistDetailViewModel>();
        builder.Services.AddTransient<ManageTicketsViewModel>();
        builder.Services.AddTransient<EoProfileViewModel>();
        builder.Services.AddTransient<CreateEventViewModel>();
        builder.Services.AddTransient<EoEventsViewModel>();
        builder.Services.AddTransient<EoGigsViewModel>();
        builder.Services.AddTransient<ArtistGigsViewModel>();
        builder.Services.AddTransient<EoAnalyticsViewModel>();
        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<ArtistDashboardViewModel>();
        builder.Services.AddTransient<ArtistVerificationViewModel>();
        builder.Services.AddTransient<OrganizerVerificationViewModel>();
        builder.Services.AddTransient<EoPortfolioViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<InvitationsViewModel>();
        builder.Services.AddTransient<MerchViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<PlatformAnalyticsViewModel>();
        builder.Services.AddTransient<VerificationsViewModel>();
        builder.Services.AddTransient<FollowingViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();
        builder.Services.AddTransient<AttendeesViewModel>();
        builder.Services.AddTransient<AdminGenresViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<ExplorePage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<EventDetailPage>();
        builder.Services.AddTransient<RadarPage>();
        builder.Services.AddTransient<TicketPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<TicketSelectionPage>();
        builder.Services.AddTransient<CheckoutPage>();
        builder.Services.AddTransient<TicketSuccessPage>();
        builder.Services.AddTransient<ArtistDetailPage>();
        builder.Services.AddTransient<ManageTicketsPage>();
        builder.Services.AddTransient<EoProfilePage>();
        builder.Services.AddTransient<CreateEventPage>();
        builder.Services.AddTransient<EoEventsPage>();
        builder.Services.AddTransient<EoGigsPage>();
        builder.Services.AddTransient<ArtistGigsPage>();
        builder.Services.AddTransient<EoAnalyticsPage>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<ArtistDashboardPage>();
        builder.Services.AddTransient<ArtistVerificationPage>();
        builder.Services.AddTransient<OrganizerVerificationPage>();
        builder.Services.AddTransient<EoPortfolioPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<InvitationsPage>();
        builder.Services.AddTransient<MerchPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<PlatformAnalyticsPage>();
        builder.Services.AddTransient<VerificationsPage>();
        builder.Services.AddTransient<FollowingPage>();
        builder.Services.AddTransient<CheckInPage>();
        builder.Services.AddTransient<AttendeesPage>();
        builder.Services.AddTransient<GenresPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Catat exception tak tertangani ke file log agar crash yang tampak
        // hanya sebagai force close (mis. stowed exception XAML di Windows)
        // tetap bisa didiagnosis. Harus dipanggil SETELAH Build() supaya
        // Microsoft.UI.Xaml.Application.Current sudah tersedia.
        CrashLogger.Attach();

        return app;
    }
}
