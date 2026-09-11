using GigRadarMobile.Models;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Membangun node radar dari daftar event, relatif ke titik pusat (user).
/// Dipakai bersama oleh teaser radar di Home dan halaman Radar.
/// </summary>
public static class RadarBuilder
{
    public static List<RadarNode> Build(
        IEnumerable<GigEvent> events,
        double centerLat,
        double centerLng,
        double maxRadiusKm,
        string? genre = null,
        bool tonightOnly = false,
        int? limit = null)
    {
        var nodes = new List<RadarNode>();

        foreach (var e in events)
        {
            if (e.Status is not ("Published" or "SoldOut")) continue;

            var distance = GeoHelper.HaversineKm(centerLat, centerLng, e.Latitude, e.Longitude);
            if (distance > maxRadiusKm) continue;

            if (tonightOnly && e.StartDate.Date != DateTime.Today) continue;

            var genreName = e.Genre?.Name ?? string.Empty;
            if (!string.IsNullOrEmpty(genre) &&
                !string.Equals(genreName, genre, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var popularity = Math.Clamp((e.ViewsCount + e.SavesCount * 3) / 5000.0, 0.15, 1.0);

            nodes.Add(new RadarNode
            {
                Event = e,
                DistanceKm = distance,
                BearingDeg = GeoHelper.BearingDeg(centerLat, centerLng, e.Latitude, e.Longitude),
                Popularity = popularity,
                Color = GenreColors.For(genreName)
            });
        }

        var sorted = nodes.OrderBy(n => n.DistanceKm).ToList();
        return limit.HasValue ? sorted.Take(limit.Value).ToList() : sorted;
    }
}