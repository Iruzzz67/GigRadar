using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Test hardening akun demo (§24): password default hanya boleh berlaku di
/// Development. Di luar itu password wajib sudah tidak bisa ditebak.
/// </summary>
public class SeedAccountHardeningTests : IDisposable
{
    private readonly AppDbContext _context;

    public SeedAccountHardeningTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    /// <summary>
    /// Akun demo dibuat manual (bukan mengandalkan HasData) supaya test deterministik
    /// dan langsung menguji logika hardening-nya.
    /// </summary>
    private void SeedDemoAccounts()
    {
        foreach (var (email, password) in SeedAccountHardening.DemoAccounts)
        {
            _context.Users.Add(new User
            {
                Name = email,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            });
        }

        // Akun non-demo dengan password yang kebetulan sama: tidak boleh ikut diubah.
        _context.Users.Add(new User
        {
            Name = "Pengguna Biasa",
            Email = "pengguna@uji.test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "User"
        });

        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private string HashOf(string email) =>
        _context.Users.First(u => u.Email == email).PasswordHash;

    private static bool PasswordWorks(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);

    [Fact]
    public void Ensure_InDevelopment_KeepsDefaultPasswords()
    {
        SeedDemoAccounts();

        var rotated = SeedAccountHardening.Ensure(_context, isDevelopment: true);

        Assert.Empty(rotated);
        Assert.True(PasswordWorks("admin123", HashOf("admin@gigradar.com")));
        Assert.True(PasswordWorks("eo123", HashOf("eo@gigradar.com")));
        Assert.True(PasswordWorks("artist123", HashOf("artist@gigradar.com")));
    }

    [Fact]
    public void Ensure_OutsideDevelopment_RotatesAllDefaultPasswords()
    {
        SeedDemoAccounts();

        var rotated = SeedAccountHardening.Ensure(_context, isDevelopment: false);

        Assert.Equal(SeedAccountHardening.DemoAccounts.Count, rotated.Count);
        Assert.False(PasswordWorks("admin123", HashOf("admin@gigradar.com")));
        Assert.False(PasswordWorks("eo123", HashOf("eo@gigradar.com")));
        Assert.False(PasswordWorks("artist123", HashOf("artist@gigradar.com")));
    }

    [Fact]
    public void Ensure_DoesNotTouchAccountsOutsideTheDemoList()
    {
        SeedDemoAccounts();

        SeedAccountHardening.Ensure(_context, isDevelopment: false);

        // Password user biasa tidak diubah meskipun kebetulan sama dengan default admin.
        Assert.True(PasswordWorks("admin123", HashOf("pengguna@uji.test")));
    }

    [Fact]
    public void Ensure_IsIdempotent()
    {
        SeedDemoAccounts();

        var first = SeedAccountHardening.Ensure(_context, isDevelopment: false);
        var second = SeedAccountHardening.Ensure(_context, isDevelopment: false);

        Assert.NotEmpty(first);
        Assert.Empty(second);

        // Password hasil rotasi tetap berlaku setelah pemanggilan kedua.
        var adminHash = HashOf("admin@gigradar.com");
        Assert.False(PasswordWorks("admin123", adminHash));
    }

    [Fact]
    public void Ensure_WithOverride_UsesOperatorPassword()
    {
        SeedDemoAccounts();

        SeedAccountHardening.Ensure(_context, isDevelopment: false, overridePassword: "RahasiaOperator#2026");

        foreach (var email in SeedAccountHardening.DemoAccounts.Keys)
            Assert.True(PasswordWorks("RahasiaOperator#2026", HashOf(email)));
    }

    [Fact]
    public void Ensure_MissingAccounts_ChangesNothing()
    {
        var rotated = SeedAccountHardening.Ensure(_context, isDevelopment: false);

        Assert.Empty(rotated);
    }

    [Fact]
    public void Ensure_BrokenPasswordHash_IsLeftAlone()
    {
        _context.Users.Add(new User
        {
            Name = "Admin",
            Email = "admin@gigradar.com",
            PasswordHash = string.Empty,   // hash rusak/kosong
            Role = "Admin"
        });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Tidak melempar exception dan tidak menandai akun sebagai "diubah".
        var rotated = SeedAccountHardening.Ensure(_context, isDevelopment: false);

        Assert.Empty(rotated);
        Assert.Equal(string.Empty, HashOf("admin@gigradar.com"));
    }
}
