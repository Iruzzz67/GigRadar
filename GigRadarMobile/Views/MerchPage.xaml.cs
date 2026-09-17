using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class MerchPage : ContentPage
{
    public MerchPage(MerchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as MerchViewModel)?.LoadCommand.Execute(null);
    }
}
