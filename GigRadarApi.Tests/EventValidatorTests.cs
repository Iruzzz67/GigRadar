using GigRadarApi.Models;
using GigRadarApi.Services;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test EventValidator — aturan validasi body event yang dipakai
/// controller sebelum create/update (nama, tanggal, harga, kapasitas, lat/lng, status).
/// </summary>
public class EventValidatorTests
{
    private static Event ValidEvent() => TestData.MakeEvent();

    [Fact]
    public void Validate_ValidEvent_ReturnsNoErrors()
    {
        Assert.Empty(EventValidator.Validate(ValidEvent()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingName_Rejected(string? name)
    {
        var evt = ValidEvent();
        evt.Name = name!;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Nama"));
    }

    [Fact]
    public void Validate_EndBeforeStart_Rejected()
    {
        var evt = ValidEvent();
        evt.EndDate = evt.StartDate.AddDays(-1);

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("EndDate"));
    }

    [Fact]
    public void Validate_NegativePrice_Rejected()
    {
        var evt = ValidEvent();
        evt.MinPrice = -1;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Harga"));
    }

    [Fact]
    public void Validate_MinAboveMax_Rejected()
    {
        var evt = ValidEvent();
        evt.MinPrice = evt.MaxPrice + 1;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("MinPrice"));
    }

    [Fact]
    public void Validate_NegativeCapacity_Rejected()
    {
        var evt = ValidEvent();
        evt.Capacity = -5;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Kapasitas"));
    }

    // ── Latitude / Longitude (temuan review 2026-09-14) ──

    [Theory]
    [InlineData(-90.0001)]  // di bawah batas
    [InlineData(90.0001)]   // di atas batas
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Validate_InvalidLatitude_Rejected(double lat)
    {
        var evt = ValidEvent();
        evt.Latitude = lat;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Latitude"));
    }

    [Theory]
    [InlineData(-180.0001)]
    [InlineData(180.0001)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Validate_InvalidLongitude_Rejected(double lng)
    {
        var evt = ValidEvent();
        evt.Longitude = lng;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Longitude"));
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public void Validate_LatitudeBoundary_Accepted(double lat)
    {
        var evt = ValidEvent();
        evt.Latitude = lat;

        Assert.Empty(EventValidator.Validate(evt));
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public void Validate_LongitudeBoundary_Accepted(double lng)
    {
        var evt = ValidEvent();
        evt.Longitude = lng;

        Assert.Empty(EventValidator.Validate(evt));
    }

    // ── Status ───────────────────────────────────────────

    [Theory]
    [InlineData("Published")]
    [InlineData("Draft")]
    [InlineData("SoldOut")]
    [InlineData("Completed")]
    [InlineData("draft")]     // case-insensitive diterima
    public void Validate_AllowedStatuses_Accepted(string status)
    {
        var evt = ValidEvent();
        evt.Status = status;

        Assert.Empty(EventValidator.Validate(evt));
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("Batal")]
    [InlineData("PublishedX")]
    public void Validate_UnknownStatus_Rejected(string status)
    {
        var evt = ValidEvent();
        evt.Status = status;

        Assert.Contains(EventValidator.Validate(evt), e => e.Contains("Status"));
    }
}
