using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Data;

/// <summary>
/// Akun demo (admin/eo/artist) di-seed bersama password default yang tertulis di
/// source code — aman untuk development, berbahaya kalau database produksi dibuat
/// oleh <c>Migrate()</c> karena kredensialnya bisa ditebak siapa pun (§24).
///
/// Helper ini menonaktifkan password default tersebut di luar Development dengan
/// menggantinya memakai password acak (atau nilai env var
/// <see cref="PasswordOverrideVariable"/> bila operator ingin password yang diketahui),
/// lalu melaporkan akun mana yang diubah agar operator tahu harus segera reset.
///
/// Idempoten: akun yang password-nya sudah bukan default tidak disentuh lagi.
/// </summary>
public static class SeedAccountHardening
{
    /// <summary>Email akun demo → password default yang dipakai seed/bootstrap.</summary>
    public static readonly IReadOnlyDictionary<string, string> DemoAccounts =
        new Dictionary<string, string>
        {
            ["admin@gigradar.com"] = "admin123",
            ["eo@gigradar.com"] = "eo123",
            ["artist@gigradar.com"] = "artist123"
        };

    /// <summary>Env var opsional: ganti password akun demo dengan nilai yang dipegang operator.</summary>
    public const string PasswordOverrideVariable = "GIGRADAR_SEED_ACCOUNT_PASSWORD";

    /// <summary>
    /// Ganti password default akun demo bila aplikasi berjalan di luar Development.
    /// Mengembalikan daftar email yang password-nya baru diacak (kosong = tidak ada perubahan).
    /// </summary>
    public static List<string> Ensure(AppDbContext context, bool isDevelopment, string? overridePassword = null)
    {
        var rotated = new List<string>();

        // Di Development akun demo memang dipakai untuk mencoba alur EO/Artist.
        if (isDevelopment) return rotated;

        foreach (var (email, defaultPassword) in DemoAccounts)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) continue;
            if (!StillUsesDefaultPassword(user.PasswordHash, defaultPassword)) continue;

            var newPassword = string.IsNullOrWhiteSpace(overridePassword)
                ? GeneratePassword()
                : overridePassword;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            rotated.Add(email);
        }

        if (rotated.Count > 0)
            context.SaveChanges();

        return rotated;
    }

    /// <summary>
    /// BCrypt.Verify melempar exception untuk hash kosong/rusak; anggap itu bukan
    /// "masih default" supaya akun yang datanya tidak wajar tidak ikut diubah.
    /// </summary>
    private static bool StillUsesDefaultPassword(string? passwordHash, string defaultPassword)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(defaultPassword, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Password acak 32 karakter — tidak bisa ditebak, tidak mengandung karakter yang menyulitkan input.</summary>
    private static string GeneratePassword()
    {
        Span<byte> bytes = stackalloc byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);

        // base64url tanpa padding: aman diketik ulang operator dari log.
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
