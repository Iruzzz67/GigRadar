using GigRadarMobile.Helpers;
using Xunit;

namespace GigRadarMobile.Tests;

public class GeoHelperTests
{
    // Pusat Jakarta — fallback aplikasi.
    private const double JakartaLat = -6.2088;
    private const double JakartaLng = 106.8456;

    [Fact]
    public void HaversineKm_SamePoint_IsZero()
    {
        var km = GeoHelper.HaversineKm(JakartaLat, JakartaLng, JakartaLat, JakartaLng);

        Assert.Equal(0, km, precision: 6);
    }

    [Fact]
    public void HaversineKm_JakartaToBandung_IsRoughly150Km()
    {
        // Bandung: -6.9175, 107.6191 — Haversine great-circle Jakarta–Bandung ± 116 km.
        var km = GeoHelper.HaversineKm(JakartaLat, JakartaLng, -6.9175, 107.6191);

        Assert.InRange(km, 100, 135);
    }

    [Fact]
    public void HaversineKm_IsSymmetric()
    {
        var a = GeoHelper.HaversineKm(JakartaLat, JakartaLng, -7.7956, 110.3695); // Yogyakarta
        var b = GeoHelper.HaversineKm(-7.7956, 110.3695, JakartaLat, JakartaLng);

        Assert.Equal(a, b, precision: 6);
    }

    [Theory]
    [InlineData(0.0005, "1 m")]     // midpoint → dibulatkan AwayFromZero
    [InlineData(0.0004, "0 m")]
    [InlineData(0.05, "50 m")]
    [InlineData(0.999, "999 m")]
    public void FormatKm_UsesMetersBelowOneKm(double km, string expected)
    {
        Assert.Equal(expected, GeoHelper.FormatKm(km));
    }

    [Theory]
    [InlineData(1.04, "1 km")]       // format "0.#" → tanpa desimal
    [InlineData(1.5, "1,5 km")]      // Kultur ID: desimal koma (Runner culture)
    [InlineData(25, "25 km")]
    public void FormatKm_UsesKmAtOneKmAndAbove(double km, string expected)
    {
        Assert.Equal(expected, GeoHelper.FormatKm(km));
    }

    [Fact]
    public void BearingDeg_North_ReturnsAroundZero()
    {
        // Titik tepat di utara pusat — hasil 360 % 360 = 0 (atau ~0 dengan toleransi).
        var bearing = GeoHelper.BearingDeg(JakartaLat, JakartaLng, JakartaLat + 0.1, JakartaLng);

        Assert.True(bearing < 1 || bearing > 359, $"bearing harus ~0, actual: {bearing}");
    }

    [Fact]
    public void BearingDeg_East_ReturnsAround90()
    {
        var bearing = GeoHelper.BearingDeg(JakartaLat, JakartaLng, JakartaLat, JakartaLng + 0.1);

        Assert.InRange(bearing, 88, 92);
    }
}
