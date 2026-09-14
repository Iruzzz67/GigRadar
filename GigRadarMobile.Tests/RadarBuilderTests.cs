using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using Xunit;

namespace GigRadarMobile.Tests;

/// <summary>
/// Unit test RadarBuilder — membangun node radar dari event.
/// Dipakai bersama oleh teaser radar Home dan halaman Radar.
/// </summary>
public class RadarBuilderTests
{
    // Pusat Jakarta — sama dengan fallback aplikasi.
    private const double CenterLat = -6.2088;
    private const double CenterLng = 106.8456;

    private static GigEvent MakeEvent(
        string name = "Gig",
        string status = "Published",
        double lat = -6.2088,
        double lng = 106.8456,
        string? genre = null,
        DateTime? startDate = null,
        int views = 0,
        int saves = 0)
    {
        return new GigEvent
        {
            EventId = Random.Shared.Next(1, int.MaxValue),
            Name = name,
            Status = status,
            Latitude = lat,
            Longitude = lng,
            Genre = genre == null ? null : new Genre { Name = genre },
            StartDate = startDate ?? DateTime.Today,
            ViewsCount = views,
            SavesCount = saves
        };
    }

    [Fact]
    public void Build_EmptyInput_ReturnsNoNodes()
    {
        var nodes = RadarBuilder.Build(Array.Empty<GigEvent>(), CenterLat, CenterLng, 25);

        Assert.Empty(nodes);
    }

    [Fact]
    public void Build_IncludesEventWithinRadius_WithDistanceAndBearing()
    {
        // ±0.01° ≈ 1.1–1.6 km dari pusat → jelas di dalam radius 25 km.
        var ev = MakeEvent(lat: -6.2188, lng: 106.8556);

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25);

        var node = Assert.Single(nodes);
        Assert.Equal(ev, node.Event);
        Assert.InRange(node.DistanceKm, 0.5, 5);
        Assert.InRange(node.BearingDeg, 0, 360);
    }

    [Fact]
    public void Build_ExcludesEventOutsideRadius()
    {
        // ±0.5° ≈ 55+ km → di luar radius 25 km.
        var ev = MakeEvent(lat: -6.7088, lng: 106.3456);

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25);

        Assert.Empty(nodes);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Build_ExcludesNonVisibleStatuses(string status)
    {
        var ev = MakeEvent(status: status);

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25);

        Assert.Empty(nodes);
    }

    [Theory]
    [InlineData("Published")]
    [InlineData("SoldOut")]
    public void Build_IncludesVisibleStatuses(string status)
    {
        var ev = MakeEvent(status: status);

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25);

        Assert.Single(nodes);
    }

    [Fact]
    public void Build_GenreFilter_IsCaseInsensitive()
    {
        var ev = MakeEvent(genre: "Indie");

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25, genre: "iNdIe");

        Assert.Single(nodes);
    }

    [Fact]
    public void Build_GenreFilter_ExcludesOtherGenres()
    {
        var ev = MakeEvent(genre: "Indie");

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25, genre: "Jazz");

        Assert.Empty(nodes);
    }

    [Fact]
    public void Build_TonightOnly_KeepsOnlyTodayEvents()
    {
        var tonight = MakeEvent(name: "Tonight", startDate: DateTime.Today);
        var nextWeek = MakeEvent(name: "NextWeek", startDate: DateTime.Today.AddDays(6));

        var nodes = RadarBuilder.Build(new[] { tonight, nextWeek }, CenterLat, CenterLng, 25, tonightOnly: true);

        var node = Assert.Single(nodes);
        Assert.Equal("Tonight", node.Event.Name);
    }

    [Fact]
    public void Build_SortsByDistance_ClosestFirst()
    {
        var far = MakeEvent(name: "Far", lat: -6.3088, lng: 106.9456);   // ~15 km
        var near = MakeEvent(name: "Near", lat: -6.2138, lng: 106.8506); // ~1 km

        var nodes = RadarBuilder.Build(new[] { far, near }, CenterLat, CenterLng, 25);

        Assert.Equal(2, nodes.Count);
        Assert.Equal("Near", nodes[0].Event.Name);
        Assert.Equal("Far", nodes[1].Event.Name);
        Assert.True(nodes[0].DistanceKm <= nodes[1].DistanceKm);
    }

    [Fact]
    public void Build_Limit_TakesClosestOnly()
    {
        var far = MakeEvent(name: "Far", lat: -6.3088, lng: 106.9456);
        var near = MakeEvent(name: "Near", lat: -6.2138, lng: 106.8506);

        var nodes = RadarBuilder.Build(new[] { far, near }, CenterLat, CenterLng, 25, limit: 1);

        var node = Assert.Single(nodes);
        Assert.Equal("Near", node.Event.Name);
    }

    [Fact]
    public void Build_Popularity_ClampedBetweenMinAndOne()
    {
        // Populer besar: (views + saves*3)/5000 > 1 → clamp ke 1.
        var popular = MakeEvent(views: 4000, saves: 1000);
        // Sepi: 0 → clamp ke floor 0.15.
        var quiet = MakeEvent(views: 0, saves: 0);

        var nodes = RadarBuilder.Build(new[] { popular, quiet }, CenterLat, CenterLng, 25);

        Assert.All(nodes, n => Assert.InRange(n.Popularity, 0.15, 1.0));
        Assert.Contains(nodes, n => n.Popularity == 1.0);
        Assert.Contains(nodes, n => n.Popularity == 0.15);
    }

    [Fact]
    public void Build_GenreColor_MatchesGenrePalette()
    {
        var ev = MakeEvent(genre: "Rock");

        var nodes = RadarBuilder.Build(new[] { ev }, CenterLat, CenterLng, 25);

        var node = Assert.Single(nodes);
        Assert.Equal(GenreColors.Rock, node.Color);
    }
}
