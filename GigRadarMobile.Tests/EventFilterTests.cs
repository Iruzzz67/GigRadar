using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using Xunit;

namespace GigRadarMobile.Tests;

/// <summary>
/// Test filter tanggal &amp; harga Explore (GIGRADAR_APP_SPEC.md §7).
/// Tanggal acuan selalu disuntik lewat parameter supaya hasilnya deterministik.
/// </summary>
public class EventFilterTests
{
    /// <summary>Rabu, 16 September 2026 — sengaja bukan akhir pekan.</summary>
    private static readonly DateTime Wednesday = new(2026, 9, 16);

    private static GigEvent MakeEvent(DateTime start, decimal minPrice = 50_000m) => new()
    {
        EventId = 1,
        Name = "Gig Uji",
        StartDate = start,
        EndDate = start.AddHours(3),
        MinPrice = minPrice,
        MaxPrice = minPrice,
        Status = "Published"
    };

    // ── Validasi opsi ────────────────────────────────────

    [Theory]
    [InlineData("Semua")]
    [InlineData("Malam ini")]
    [InlineData("Besok")]
    [InlineData("Minggu ini")]
    [InlineData("Akhir pekan")]
    public void IsValidDateOption_AcceptsKnownLabels(string option)
    {
        Assert.True(EventFilter.IsValidDateOption(option));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Hari ini")]     // label lama yang sengaja tidak dipakai lagi
    [InlineData("custom")]
    public void IsValidDateOption_RejectsUnknownLabels(string? option)
    {
        Assert.False(EventFilter.IsValidDateOption(option));
    }

    [Theory]
    [InlineData("Semua")]
    [InlineData("Gratis")]
    [InlineData("<50rb")]
    [InlineData("<100rb")]
    public void IsValidPriceOption_AcceptsKnownLabels(string option)
    {
        Assert.True(EventFilter.IsValidPriceOption(option));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("murah")]
    [InlineData("<50")]
    public void IsValidPriceOption_RejectsUnknownLabels(string? option)
    {
        Assert.False(EventFilter.IsValidPriceOption(option));
    }

    // ── Tanggal ──────────────────────────────────────────

    [Fact]
    public void MatchesDate_Today_OnlyMatchesToday()
    {
        var today = MakeEvent(Wednesday.AddHours(20));
        var tomorrow = MakeEvent(Wednesday.AddDays(1).AddHours(20));

        Assert.True(EventFilter.MatchesDate(today, "Malam ini", Wednesday));
        Assert.False(EventFilter.MatchesDate(tomorrow, "Malam ini", Wednesday));
    }

    [Fact]
    public void MatchesDate_Tomorrow_OnlyMatchesNextDay()
    {
        var today = MakeEvent(Wednesday.AddHours(20));
        var tomorrow = MakeEvent(Wednesday.AddDays(1).AddHours(20));
        var dayAfter = MakeEvent(Wednesday.AddDays(2).AddHours(20));

        Assert.False(EventFilter.MatchesDate(today, "Besok", Wednesday));
        Assert.True(EventFilter.MatchesDate(tomorrow, "Besok", Wednesday));
        Assert.False(EventFilter.MatchesDate(dayAfter, "Besok", Wednesday));
    }

    [Fact]
    public void MatchesDate_ThisWeek_CoversSevenDaysInclusive()
    {
        var inSixDays = MakeEvent(Wednesday.AddDays(6).AddHours(19));
        var inSevenDays = MakeEvent(Wednesday.AddDays(7).AddHours(19));
        var inEightDays = MakeEvent(Wednesday.AddDays(8).AddHours(19));
        var yesterday = MakeEvent(Wednesday.AddDays(-1).AddHours(19));

        Assert.True(EventFilter.MatchesDate(inSixDays, "Minggu ini", Wednesday));
        Assert.True(EventFilter.MatchesDate(inSevenDays, "Minggu ini", Wednesday));
        Assert.False(EventFilter.MatchesDate(inEightDays, "Minggu ini", Wednesday));
        Assert.False(EventFilter.MatchesDate(yesterday, "Minggu ini", Wednesday));
    }

    [Fact]
    public void MatchesDate_Weekend_CoversTodayThroughSunday()
    {
        // Acuan Rabu, 16 Sep 2026 → akhir pekan berakhir Minggu, 20 Sep 2026.
        var today = MakeEvent(Wednesday.AddHours(20));
        var friday = MakeEvent(new DateTime(2026, 9, 18, 20, 0, 0));
        var saturday = MakeEvent(new DateTime(2026, 9, 19, 20, 0, 0));
        var sunday = MakeEvent(new DateTime(2026, 9, 20, 15, 0, 0));
        var nextWednesday = MakeEvent(new DateTime(2026, 9, 23, 20, 0, 0));
        var yesterday = MakeEvent(Wednesday.AddDays(-1).AddHours(20));

        Assert.True(EventFilter.MatchesDate(today, "Akhir pekan", Wednesday));
        Assert.True(EventFilter.MatchesDate(friday, "Akhir pekan", Wednesday));
        Assert.True(EventFilter.MatchesDate(saturday, "Akhir pekan", Wednesday));
        Assert.True(EventFilter.MatchesDate(sunday, "Akhir pekan", Wednesday));
        Assert.False(EventFilter.MatchesDate(nextWednesday, "Akhir pekan", Wednesday));
        Assert.False(EventFilter.MatchesDate(yesterday, "Akhir pekan", Wednesday));
    }

    [Fact]
    public void MatchesDate_Weekend_FromSaturdayReference_EndsOnSunday()
    {
        // Acuan Sabtu, 19 Sep 2026 → hanya s/d Minggu, 20 Sep 2026.
        var saturday = new DateTime(2026, 9, 19);
        var sunday = MakeEvent(new DateTime(2026, 9, 20, 15, 0, 0));
        var monday = MakeEvent(new DateTime(2026, 9, 21, 20, 0, 0));
        var friday = MakeEvent(new DateTime(2026, 9, 18, 20, 0, 0));

        Assert.True(EventFilter.MatchesDate(MakeEvent(saturday.AddHours(20)), "Akhir pekan", saturday));
        Assert.True(EventFilter.MatchesDate(sunday, "Akhir pekan", saturday));
        Assert.False(EventFilter.MatchesDate(monday, "Akhir pekan", saturday));
        Assert.False(EventFilter.MatchesDate(friday, "Akhir pekan", saturday));
    }

    [Fact]
    public void MatchesDate_NoFilter_KeepsEverything()
    {
        var past = MakeEvent(Wednesday.AddDays(-30));

        Assert.True(EventFilter.MatchesDate(past, "Semua", Wednesday));
        Assert.True(EventFilter.MatchesDate(past, null, Wednesday));
    }

    // ── Harga ────────────────────────────────────────────

    [Fact]
    public void MatchesPrice_Free_OnlyZeroPrice()
    {
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 0), "Gratis"));
        Assert.False(EventFilter.MatchesPrice(MakeEvent(Wednesday, 1), "Gratis"));
    }

    [Fact]
    public void MatchesPrice_UnderFiftyThousand_IncludesFreeButNotBoundary()
    {
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 0), "<50rb"));          // gratis ikut
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 49_999), "<50rb"));
        Assert.False(EventFilter.MatchesPrice(MakeEvent(Wednesday, 50_000), "<50rb"));    // batas eksklusif
    }

    [Fact]
    public void MatchesPrice_UnderHundredThousand_IncludesCheaperTiers()
    {
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 0), "<100rb"));
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 75_000), "<100rb"));
        Assert.False(EventFilter.MatchesPrice(MakeEvent(Wednesday, 100_000), "<100rb"));
        Assert.False(EventFilter.MatchesPrice(MakeEvent(Wednesday, 150_000), "<100rb"));
    }

    [Fact]
    public void MatchesPrice_NoFilter_KeepsEverything()
    {
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 999_000), "Semua"));
        Assert.True(EventFilter.MatchesPrice(MakeEvent(Wednesday, 999_000), null));
    }
}
