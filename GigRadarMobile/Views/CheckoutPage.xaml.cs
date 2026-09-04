using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class CheckoutPage : ContentPage
{
    public CheckoutPage(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}