using GigRadarMobile.Models;
using Xunit;

namespace GigRadarMobile.Tests;

/// <summary>
/// Aturan kelayakan marketplace gig (research §5/§6): kapan tombol "Lamar"
/// aktif, dan status mana yang masih boleh di-shortlist/tolak/ditarik.
/// Aturan ini harus sama dengan validasi server, jadi dijaga lewat test.
/// </summary>
public class GigMarketplaceTests
{
    private static GigItem MakeGig(string status = "OPEN", bool hasApplied = false) => new()
    {
        GigId = 1,
        EventId = 1,
        Name = "Opening set",
        Status = status,
        HasApplied = hasApplied,
        RequiredArtistCount = 2,
        ApplicantCount = 3
    };

    [Fact]
    public void CanApply_OnlyWhenOpenAndNotApplied()
    {
        Assert.True(MakeGig("OPEN").CanApply);
        Assert.False(MakeGig("OPEN", hasApplied: true).CanApply);
        Assert.False(MakeGig("DRAFT").CanApply);
        Assert.False(MakeGig("CLOSED").CanApply);
    }

    [Theory]
    [InlineData("OPEN", "Terbuka")]
    [InlineData("DRAFT", "Draf")]
    [InlineData("CLOSED", "Ditutup")]
    public void StatusLabel_MapsBackendStatuses(string status, string expected)
    {
        Assert.Equal(expected, MakeGig(status).StatusLabel);
    }

    [Fact]
    public void ApplyLabel_ReflectsAppliedAndClosedStates()
    {
        Assert.Equal("Lamar gig", MakeGig("OPEN").ApplyLabel);
        Assert.Equal("Sudah melamar", MakeGig("OPEN", hasApplied: true).ApplyLabel);
        Assert.Equal("Ditutup", MakeGig("CLOSED").ApplyLabel);
    }

    [Fact]
    public void StatusActionLabel_TogglesBetweenOpenAndClosed()
    {
        Assert.Equal("Tutup lamaran", MakeGig("OPEN").StatusActionLabel);
        Assert.Equal("Buka lamaran", MakeGig("DRAFT").StatusActionLabel);
        Assert.Equal("Buka lamaran", MakeGig("CLOSED").StatusActionLabel);
    }

    [Fact]
    public void ApplicantLabel_ShowsApplicantsAndSlots()
    {
        Assert.Equal("3 pelamar · 2 slot", MakeGig().ApplicantLabel);
    }

    [Fact]
    public void BudgetLabel_CollapsesEqualBounds()
    {
        var gig = MakeGig();
        gig.BudgetMin = 500_000;
        gig.BudgetMax = 500_000;
        Assert.Equal("Rp 500.000", gig.BudgetLabel);

        gig.BudgetMax = 1_000_000;
        Assert.Equal("Rp 500.000–1.000.000", gig.BudgetLabel);
    }

    [Theory]
    [InlineData("PENDING", true, true, true)]
    [InlineData("SHORTLISTED", false, true, true)]
    [InlineData("REJECTED", false, false, false)]
    [InlineData("WITHDRAWN", false, false, false)]
    public void ApplicationActions_FollowStatusRules(
        string status, bool canShortlist, bool canReject, bool canWithdraw)
    {
        var application = new GigApplicationItem { Status = status };

        Assert.Equal(canShortlist, application.CanShortlist);
        Assert.Equal(canReject, application.CanReject);
        Assert.Equal(canWithdraw, application.CanWithdraw);
    }

    [Theory]
    [InlineData("PENDING", "Menunggu")]
    [InlineData("SHORTLISTED", "Shortlist")]
    [InlineData("REJECTED", "Ditolak")]
    [InlineData("WITHDRAWN", "Ditarik")]
    public void ApplicationStatusLabel_MapsBackendStatuses(string status, string expected)
    {
        Assert.Equal(expected, new GigApplicationItem { Status = status }.StatusLabel);
    }

    [Fact]
    public void TargetLabel_CombinesGigAndEvent()
    {
        var application = new GigApplicationItem { GigName = "Opening set", EventName = "Festival" };
        Assert.Equal("Opening set · Festival", application.TargetLabel);

        application.EventName = "";
        Assert.Equal("Opening set", application.TargetLabel);
    }

    [Fact]
    public void ArtistLabel_SkipsEmptyParts()
    {
        var application = new GigApplicationItem { ArtistGenre = "Shoegaze", ArtistCity = "Jakarta" };
        Assert.Equal("Shoegaze • Jakarta", application.ArtistLabel);

        application.ArtistCity = "";
        Assert.Equal("Shoegaze", application.ArtistLabel);
    }
}
