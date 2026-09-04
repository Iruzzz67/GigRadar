using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage(UsersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsersViewModel vm)
        {
            vm.LoadCommand.Execute(null);
        }
    }
}