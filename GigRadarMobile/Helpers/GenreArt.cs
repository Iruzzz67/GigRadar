using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Memetakan genre ke keluarga warna panggung (lihat Colors.xaml: StageViolet,
/// StageCyan, StageEmber, StagePink, StageAmber, PrimaryColor).
///
/// Tujuan: daftar event/artist tidak lagi seragam satu warna. Aksen tetap
/// aksen — hanya dipakai untuk garis, badge, dan teks kecil — sehingga
/// hierarki dan kontras teks utama tidak berubah.
///
/// Pakai lewat <c>ConverterParameter</c>:
///   <c>ConverterParameter=accent</c>  → warna aksen penuh (badge/garis/ikon)
///   <c>ConverterParameter=tint</c>    → latar gelap untuk chip/badge
/// </summary>
public class GenreAccentConverter : IValueConverter
{
    // Urutan penting: pola paling spesifik dulu.
    private static readonly (string Pattern, string Accent, string Tint)[] Map =
    {
        ("shoegaze", "#8B5CF6", "#1E1735"),
        ("dream",    "#8B5CF6", "#1E1735"),
        ("emo",      "#FF4D8D", "#2E1424"),
        ("punk",     "#FB7185", "#2A1219"),
        ("hardcore", "#FB7185", "#2A1219"),
        ("metal",    "#FB7185", "#2A1219"),
        ("math",     "#22D3EE", "#0E2830"),
        ("post",     "#22D3EE", "#0E2830"),
        ("electronic", "#22D3EE", "#0E2830"),
        ("hip hop",  "#F6C453", "#2B2110"),
        ("jazz",     "#F6C453", "#2B2110"),
        ("folk",     "#F6C453", "#2B2110"),
        ("indie",    "#A3FF12", "#1B260D"),
        ("rock",     "#A3FF12", "#1B260D"),
        ("alternative", "#A3FF12", "#1B260D"),
        ("pop",      "#FF4D8D", "#2E1424"),
    };

    private const string DefaultAccent = "#A3FF12";
    private const string DefaultTint = "#1B260D";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var genre = (value as string)?.Trim() ?? string.Empty;
        var mode = (parameter as string)?.Trim().ToLowerInvariant();

        var entry = Map.FirstOrDefault(m =>
            genre.Contains(m.Pattern, StringComparison.OrdinalIgnoreCase));

        var hex = mode == "tint"
            ? (entry.Tint ?? DefaultTint)
            : (entry.Accent ?? DefaultAccent);

        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Mengembalikan tanggal relatif singkat ("Hari ini", "Besok", "3 hari lagi")
/// untuk memberi konteks waktu pada daftar event. Jatuh ke format tanggal bila
/// sudah lewat atau lebih dari sepekan.
/// </summary>
public class RelativeDayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime when) return string.Empty;

        var days = (when.Date - DateTime.Today).Days;
        return days switch
        {
            0 => "Hari ini",
            1 => "Besok",
            2 => "Lusa",
            > 2 and <= 7 => $"{days} hari lagi",
            _ => when.ToString("dd MMM", new CultureInfo("id-ID")),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
