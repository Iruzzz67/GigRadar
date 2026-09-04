using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class TicketSelectionPage : ContentPage
{
    public TicketSelectionPage(TicketSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TicketSelectionViewModel vm)
            await vm.LoadTypesCommand.ExecuteAsync(null);
    }
}