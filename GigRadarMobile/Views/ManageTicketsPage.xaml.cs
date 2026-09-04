using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ManageTicketsPage : ContentPage
{
    public ManageTicketsPage(ManageTicketsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ManageTicketsViewModel vm)
            await vm.LoadEventsCommand.ExecuteAsync(null);
    }
}