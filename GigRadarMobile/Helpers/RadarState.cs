namespace GigRadarMobile.Helpers;

/// <summary>
/// State ringan lintas-tab: chip genre di Home bisa langsung membuka tab Radar
/// dengan genre tertentu sudah terpilih. Di-reset saat user memilih "Semua".
/// </summary>
public static class RadarState
{
    public static string? SelectedGenre { get; set; }
}