using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>True → opacity redup (0.45), False → penuh (1). Untuk baris non-aktif (mis. SOLD OUT).</summary>
public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 0.45d : 1d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}