using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Mengubah url poster/foto (string) menjadi ImageSource.
/// Kalau url dari API kosong (belum ada foto diupload EO), jatuh ke placeholder
/// lokal supaya card/poster tidak pernah tampil kosong/blank.
/// </summary>
public class PosterImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var url = value as string;
        if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            var comma = url.IndexOf(',');
            if (comma > 0)
            {
                var bytes = System.Convert.FromBase64String(url[(comma + 1)..]);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }

        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return ImageSource.FromUri(uri);

        return ImageSource.FromFile("poster_placeholder");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
