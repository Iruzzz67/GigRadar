using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Mengubah url poster/foto (string) menjadi ImageSource.
///
/// Bila url dari API kosong (foto belum diunggah), jatuh ke salah satu artwork
/// default yang dipilih deterministik dari <c>ConverterParameter</c> — biasanya
/// nama artist/event — sehingga tidak ada kartu yang tampil blank dan tiap
/// entitas tetap punya visual yang berbeda. Lihat <see cref="BandArt"/>.
/// </summary>
public class PosterImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = parameter as string;
        return BandArt.Resolve(value as string, key);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
