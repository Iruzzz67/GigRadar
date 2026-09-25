using GigRadarMobile.ViewModels;

namespace GigRadarMobile.Views;

public partial class CheckInPage : ContentPage
{
    private readonly CheckInViewModel _viewModel;
    private readonly QrScannerView _scanner;

    public CheckInPage(CheckInViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;

        // Kamera dibuat di code-behind karena ZXing tidak mendukung kamera di Windows —
        // lihat QrScannerView untuk fallback-nya.
        _scanner = new QrScannerView();
        _scanner.BarcodeDetected += OnBarcodeDetected;
        ScannerHost.Content = _scanner;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.LoadCountsCommand.Execute(null);
        _scanner.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Kamera dimatikan saat halaman ditinggalkan (hemat baterai & privasi).
        _scanner.Stop();
    }

    private void OnBarcodeDetected(object? sender, string code)
    {
        // Event deteksi datang dari thread kamera — pindahkan ke UI thread sebelum
        // menyentuh properti ViewModel.
        Dispatcher.Dispatch(async () => await _viewModel.ValidateAsync(code));
    }

    private void OnTorchClicked(object? sender, EventArgs e) => _scanner.ToggleTorch();
}
