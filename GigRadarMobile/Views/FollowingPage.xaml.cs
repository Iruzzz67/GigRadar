using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class FollowingPage : ContentPage
{
    public FollowingPage(FollowingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as FollowingViewModel)?.LoadCommand.Execute(null);
    }
}
