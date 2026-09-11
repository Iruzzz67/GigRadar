namespace GigRadarMobile.Models;

public sealed record UserLocation(
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    DateTime Timestamp);
