using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class AttendeesPage : ContentPage
{
    public AttendeesPage(AttendeesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Muat ulang setiap kali halaman tampil: peserta bisa bertambah karena penjualan,
        // dan status check-in bisa berubah dari halaman check-in.
        (BindingContext as AttendeesViewModel)?.LoadCommand.Execute(null);
    }
}
