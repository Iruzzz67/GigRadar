namespace GigRadarMobile.Helpers;

/// <summary>
/// Warna genre GIGRADAR — dipakai konsisten di node radar, chip genre, dan label.
/// Palet redup tapi tetap bisa dibedakan (bukan pelangi neon).
/// </summary>
public static class GenreColors
{
    public static readonly Color Indie = Color.FromArgb("#F4F4F5");
    public static readonly Color Alternative = Color.FromArgb("#A3FF12");
    public static readonly Color Rock = Color.FromArgb("#E4572E");
    public static readonly Color Metal = Color.FromArgb("#9CA3AF");
    public static readonly Color Punk = Color.FromArgb("#E11D48");
    public static readonly Color Hardcore = Color.FromArgb("#EF476F");
    public static readonly Color Shoegaze = Color.FromArgb("#7DD3FC");
    public static readonly Color Emo = Color.FromArgb("#C084FC");
    public static readonly Color Jazz = Color.FromArgb("#F2C14E");
    public static readonly Color Folk = Color.FromArgb("#D9A05B");
    public static readonly Color Electronic = Color.FromArgb("#2DD4BF");
    public static readonly Color Pop = Color.FromArgb("#F472B6");
    public static readonly Color HipHop = Color.FromArgb("#A78BFA");
    public static readonly Color DefaultGenre = Color.FromArgb("#A1A1AA");

    public static Color For(string? genre)
    {
        if (string.IsNullOrWhiteSpace(genre)) return DefaultGenre;

        var g = genre.Trim().ToLowerInvariant();
        return g switch
        {
            "indie" => Indie,
            "alternative" or "alt" => Alternative,
            "rock" => Rock,
            "metal" => Metal,
            "punk" => Punk,
            "hardcore" => Hardcore,
            "shoegaze" => Shoegaze,
            "emo" => Emo,
            "jazz" => Jazz,
            "folk" => Folk,
            "electronic" or "edm" or "techno" or "house" => Electronic,
            "pop" => Pop,
            "hip-hop" or "hip hop" or "hiphop" or "rap" => HipHop,
            _ => DefaultGenre
        };
    }
}