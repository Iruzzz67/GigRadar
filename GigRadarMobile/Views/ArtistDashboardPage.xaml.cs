using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ArtistDashboardPage : ContentPage
{
    public ArtistDashboardPage(ArtistDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ArtistDashboardViewModel)?.Load();
    }
}