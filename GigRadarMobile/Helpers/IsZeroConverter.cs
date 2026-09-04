using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>Mengubah angka menjadi bool — true bila nilainya 0.</summary>
public class IsZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i == 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}