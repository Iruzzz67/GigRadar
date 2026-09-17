using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class EoPortfolioPage : ContentPage
{
    public EoPortfolioPage(EoPortfolioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as EoPortfolioViewModel)?.LoadCommand.Execute(null);
    }
}
