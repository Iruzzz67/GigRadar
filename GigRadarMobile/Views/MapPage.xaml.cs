using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class MapPage : ContentPage
{
    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MapViewModel vm)
            await vm.LoadNearbyEventsCommand.ExecuteAsync(null);
    }
}
