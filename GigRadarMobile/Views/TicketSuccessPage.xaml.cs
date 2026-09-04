using GigRadarMobile.Helpers;
using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class TicketSuccessPage : ContentPage
{
    public TicketSuccessPage(TicketSuccessViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TicketSuccessViewModel.QRCode))
                UpdateBarcode();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateBarcode();
    }

    private void UpdateBarcode()
    {
        if (BindingContext is TicketSuccessViewModel vm)
            BarcodeView.Drawable = new TicketBarcodeDrawable { Code = vm.QRCode };
    }
}