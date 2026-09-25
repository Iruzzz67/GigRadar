using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace GigRadarMobile.Views;

/// <summary>
/// Pembungkus kamera pemindai QR untuk halaman check-in EO (§14).
///
/// Dibuat dari kode (bukan XAML) dengan sengaja: ZXing.Net.Maui tidak mendukung
/// kamera di Windows, sedangkan aplikasi ini juga dirilis sebagai aplikasi desktop.
/// Dengan cara ini halaman check-in tetap satu file untuk semua platform — di
/// platform tanpa kamera view ini hanya menampilkan petunjuk, dan kode tiket bisa
/// dimasukkan manual.
/// </summary>
public class QrScannerView : ContentView
{
    private readonly CameraBarcodeReaderView? _reader;

    /// <summary>Dipicu setiap kali QR terbaca (di thread kamera — pemanggil wajib marshal ke UI thread).</summary>
    public event EventHandler<string>? BarcodeDetected;

    public QrScannerView()
    {
        // BarcodeScanning.IsSupported == false di Windows atau perangkat tanpa kamera.
        if (!BarcodeScanning.IsSupported)
        {
            Content = BuildUnavailablePlaceholder();
            return;
        }

        _reader = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                // Tiket GigRadar berupa QR, tapi format 1D ikut dibaca agar tiket
                // dari platform lain (atau kode batang cetak) tetap bisa dipindai.
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false,
                // Jeda antar deteksi menahan pemindaian berulang atas QR yang sama.
                DelayBetweenContinuousScans = 1500
            }
        };

        _reader.BarcodesDetected += OnBarcodesDetected;
        Content = _reader;
    }

    public bool IsCameraAvailable => _reader != null;

    public void Start()
    {
        if (_reader != null) _reader.IsDetecting = true;
    }

    public void Stop()
    {
        if (_reader != null) _reader.IsDetecting = false;
    }

    public void ToggleTorch()
    {
        if (_reader != null) _reader.IsTorchOn = !_reader.IsTorchOn;
    }

    private static View BuildUnavailablePlaceholder()
    {
        var label = new Label
        {
            Text = "Kamera pemindai tidak tersedia di platform ini.\n" +
                   "Masukkan kode tiket secara manual di kolom bawah.",
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            FontSize = 13,
            TextColor = Color.FromArgb("#A1A1AA")
        };

        return new Grid
        {
            BackgroundColor = Color.FromArgb("#1A1720"),
            Children = { label }
        };
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var value = e.Results?.FirstOrDefault()?.Value;
        if (!string.IsNullOrWhiteSpace(value))
            BarcodeDetected?.Invoke(this, value);
    }
}
