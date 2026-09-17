using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class PlatformAnalyticsPage : ContentPage
{
    public PlatformAnalyticsPage(PlatformAnalyticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlatformAnalyticsViewModel vm)
        {
            vm.LoadCommand.Execute(null);
        }
    }
}
