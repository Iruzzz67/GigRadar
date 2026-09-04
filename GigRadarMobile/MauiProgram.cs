using GigRadarMobile.Services;
using GigRadarMobile.ViewModels;
using GigRadarMobile.Views;
using Microsoft.Extensions.Logging;

namespace GigRadarMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<EventDetailViewModel>();
        builder.Services.AddTransient<MapViewModel>();
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
        builder.Services.AddTransient<EoAnalyticsViewModel>();
        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<ArtistDashboardViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<EventDetailPage>();
        builder.Services.AddTransient<MapPage>();
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
        builder.Services.AddTransient<EoAnalyticsPage>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<ArtistDashboardPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
