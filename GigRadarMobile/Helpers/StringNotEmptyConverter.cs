using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>Mengubah string menjadi bool — true bila string tidak null/kosong.</summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}