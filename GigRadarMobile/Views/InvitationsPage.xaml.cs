using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class InvitationsPage : ContentPage
{
    public InvitationsPage(InvitationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as InvitationsViewModel)?.LoadCommand.Execute(null);
    }
}
