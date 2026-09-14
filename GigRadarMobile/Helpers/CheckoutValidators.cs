namespace GigRadarMobile.Helpers;

/// <summary>
/// Aturan verifikasi data diri pembeli tiket (nama, telepon, email, umur).
/// Logika murni — dipakai CheckoutViewModel dan bisa di-unit-test tanpa UI.
/// </summary>
public static class CheckoutValidators
{
    /// <summary>Umur minimal untuk membeli tiket (aturan acara 17+).</summary>
    public const int MinimumAge = 17;

    /// <summary>
    /// Hitung umur (tahun penuh) pada hari ini — memperhitungkan ulang tahun
    /// yang belum lewat tahun ini.
    /// </summary>
    public static int CalculateAge(DateTime birthDate, DateTime today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    /// <summary>Umur pada hari ini.</summary>
    public static int CalculateAge(DateTime birthDate) => CalculateAge(birthDate, DateTime.Today);

    /// <summary>Nama valid: minimal 3 karakter setelah trim.</summary>
    public static bool IsValidFullName(string? fullName)
        => !string.IsNullOrWhiteSpace(fullName) && fullName.Trim().Length >= 3;

    /// <summary>Telepon valid: minimal 9 digit (spasi/tanda baca diabaikan).</summary>
    public static bool IsValidPhone(string? phone)
    {
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length >= 9;
    }

    /// <summary>Email valid: berisi '@' dan '.' (pemeriksaan format ringan yang sudah ada).</summary>
    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
}
