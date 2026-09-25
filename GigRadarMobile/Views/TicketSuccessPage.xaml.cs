using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class TicketSuccessPage : ContentPage
{
    public TicketSuccessPage(TicketSuccessViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    // QR tiket dibuat di TicketSuccessViewModel (QrImageFactory) dan di-bind langsung
    // lewat XAML — tidak ada lagi barcode yang digambar manual di code-behind.
}
