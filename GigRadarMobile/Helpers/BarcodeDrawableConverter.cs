using System.Globalization;

namespace GigRadarMobile.Helpers;

public class BarcodeDrawableConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new TicketBarcodeDrawable { Code = value as string ?? string.Empty };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}