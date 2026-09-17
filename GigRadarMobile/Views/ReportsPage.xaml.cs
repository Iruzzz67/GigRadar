using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class ReportsPage : ContentPage
{
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ReportsViewModel)?.LoadCommand.Execute(null);
    }
}
