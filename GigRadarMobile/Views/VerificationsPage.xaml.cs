using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class VerificationsPage : ContentPage
{
    public VerificationsPage(VerificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as VerificationsViewModel)?.LoadCommand.Execute(null);
    }
}
