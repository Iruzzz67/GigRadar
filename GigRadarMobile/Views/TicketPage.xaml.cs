using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class TicketPage : ContentPage
{
    public TicketPage(TicketViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TicketViewModel vm)
            await vm.LoadTicketsCommand.ExecuteAsync(null);
    }
}
