using GigRadarMobile.Helpers;
using Xunit;

namespace GigRadarMobile.Tests;

public class CheckoutValidatorsTests
{
    // Tanggal referensi tetap agar hasil deterministik — TO DAY (bukan hardcode)
    // supaya kasus "ulang tahun hari ini" selalu benar kapan pun test dijalankan.
    private static readonly DateTime Today = DateTime.Today;

    // ── CalculateAge ─────────────────────────────────────

    [Fact]
    public void CalculateAge_ComputesFullYearsCorrectly()
    {
        // Relatif ke Today agar deterministik kapan pun dijalankan.
        Assert.Equal(26, CheckoutValidators.CalculateAge(Today.AddYears(-26).AddDays(-1), Today));  // ulang tahun kemarin
        Assert.Equal(26, CheckoutValidators.CalculateAge(Today.AddYears(-26), Today));              // ulang tahun hari ini → sudah bertambah
        Assert.Equal(25, CheckoutValidators.CalculateAge(Today.AddYears(-26).AddDays(1), Today));   // ulang tahun besok → belum
        Assert.Equal(16, CheckoutValidators.CalculateAge(Today.AddYears(-17).AddDays(1), Today));   // belum 17
        Assert.Equal(17, CheckoutValidators.CalculateAge(Today.AddYears(-17), Today));              // tepat 17 hari ini
    }

    [Fact]
    public void CalculateAge_LeapYearBirthday_Feb29()
    {
        // Lahir 29 Feb 2008; pada 28 Feb 2027 ulang tahun ke-19 belum lewat → 18.
        var age = CheckoutValidators.CalculateAge(new DateTime(2008, 2, 29), new DateTime(2027, 2, 28));
        Assert.Equal(18, age);

        // Pada 1 Mar 2027 sudah 19.
        age = CheckoutValidators.CalculateAge(new DateTime(2008, 2, 29), new DateTime(2027, 3, 1));
        Assert.Equal(19, age);
    }

    [Fact]
    public void CalculateAge_BirthDateInFuture_ReturnsNegative()
    {
        var age = CheckoutValidators.CalculateAge(new DateTime(2030, 1, 1), Today);
        Assert.True(age < 0);
    }

    // ── Aturan umur minimal 17+ ──────────────────────────

    [Fact]
    public void MinimumAge_Is17()
    {
        Assert.Equal(17, CheckoutValidators.MinimumAge);
    }

    [Fact]
    public void AgeRule_ExactBoundary()
    {
        // 17 besok → belum boleh; 17 tepat hari ini → boleh.
        var notYet = CheckoutValidators.CalculateAge(Today.AddYears(-17).AddDays(1), Today);
        var exactly = CheckoutValidators.CalculateAge(Today.AddYears(-17), Today);

        Assert.False(notYet >= CheckoutValidators.MinimumAge);
        Assert.True(exactly >= CheckoutValidators.MinimumAge);
    }

    // ── IsValidFullName ─────────────────────────────────

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Ab", false)]        // < 3 karakter
    [InlineData(" Budi ", true)]     // trim → 4 karakter
    [InlineData("Budi Santoso", true)]
    public void IsValidFullName_ChecksLengthAfterTrim(string? fullName, bool expected)
    {
        Assert.Equal(expected, CheckoutValidators.IsValidFullName(fullName));
    }

    // ── IsValidPhone ─────────────────────────────────────

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("12345678", false)]        // 8 digit
    [InlineData("123456789", true)]        // 9 digit — batas bawah
    [InlineData("0812-3456-789", true)]    // format dengan pemisah → 11 digit
    [InlineData("+62 812 3456 7890", true)]
    [InlineData("abcdefghij", false)]      // tidak ada digit
    public void IsValidPhone_RequiresAtLeast9Digits(string? phone, bool expected)
    {
        Assert.Equal(expected, CheckoutValidators.IsValidPhone(phone));
    }

    // ── IsValidEmail ─────────────────────────────────────

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("budi", false)]
    [InlineData("budi@mail", false)]       // ada @ tapi tanpa titik
    [InlineData("budi.mail.com", false)]   // ada titik tanpa @
    [InlineData("budi@mail.com", true)]
    [InlineData("budi.santoso@mail.co.id", true)]
    public void IsValidEmail_MatchesExistingLightRule(string? email, bool expected)
    {
        Assert.Equal(expected, CheckoutValidators.IsValidEmail(email));
    }
}
