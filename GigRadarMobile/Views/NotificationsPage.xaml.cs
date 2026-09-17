using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as NotificationsViewModel)?.LoadCommand.Execute(null);
    }
}
