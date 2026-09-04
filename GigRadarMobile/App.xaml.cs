using GigRadarMobile.Helpers;
using GigRadarMobile.Services;
using GigRadarMobile.ViewModels;
using GigRadarMobile.Views;

namespace GigRadarMobile;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var auth = ServiceProvider.GetRequiredService<AuthService>();

        // Role non-User (EO/Admin/Artist) langsung ke shell-nya (tanpa onboarding genre).
        // Role User perlu onboarding genre dulu sebelum masuk UserShell.
        if (auth.IsLoggedIn && (auth.GetUserRole() != "User" || auth.IsOnboardingDone()))
        {
            return new Window(ShellRouter.CreateForRole(auth.GetUserRole()));
        }
        else
        {
            var loginVm = ServiceProvider.GetRequiredService<LoginViewModel>();
            return new Window(new NavigationPage(new LoginPage(loginVm)));
        }
    }
}
