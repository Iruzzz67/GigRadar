namespace GigRadarMobile.Helpers;

/// <summary>Utilitas geografi: jarak Haversine, bearing, dan format jarak.</summary>
public static class GeoHelper
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>Jarak antara dua koordinat (km), formula Haversine.</summary>
    public static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLng = ToRad(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    /// <summary>Bearing dari titik 1 ke titik 2 (derajat, 0 = utara, searah jarum jam).</summary>
    public static double BearingDeg(double lat1, double lng1, double lat2, double lng2)
    {
        var φ1 = ToRad(lat1);
        var φ2 = ToRad(lat2);
        var Δλ = ToRad(lng2 - lng1);

        var y = Math.Sin(Δλ) * Math.Cos(φ2);
        var x = Math.Cos(φ1) * Math.Sin(φ2) - Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);

        var bearing = (ToDeg(Math.Atan2(y, x)) + 360) % 360;
        return bearing;
    }

    public static string FormatKm(double km)
        => km < 1 ? $"{Math.Round(km * 1000, MidpointRounding.AwayFromZero)} m" : $"{km:0.#} km";

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
    private static double ToDeg(double rad) => rad * 180.0 / Math.PI;
}