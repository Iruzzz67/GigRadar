using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Test CRUD akun oleh Admin (§4.4, §19, §23): create/update/suspend/delete beserta
/// pengamannya (email unik, password di-hash, tidak bisa menghapus diri sendiri atau
/// Admin terakhir, akun dinonaktifkan tidak bisa login).
/// </summary>
public class AdminAccountTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AdminAccountTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _authService = new AuthService(_context, Configuration());
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private static IConfiguration Configuration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "kunci-uji-yang-cukup-panjang-untuk-hmac-256-bit",
            ["JwtSettings:Issuer"] = "GigRadarTest",
            ["JwtSettings:Audience"] = "GigRadarTest",
            ["JwtSettings:ExpirationMinutes"] = "60"
        }).Build();

    /// <summary>
    /// Id akun diambil dari hasil insert (bukan angka tetap) karena provider InMemory
    /// tidak menerapkan seed HasData — database uji berisi hanya data yang dibuat test.
    /// </summary>
    private async Task<int> SeedUserIdAsync(string email, string role = "User", string roleStatus = "None", string password = "rahasia123")
    {
        var user = new User
        {
            Name = "Akun Uji",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            RoleStatus = roleStatus,
            City = "Jakarta"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return user.UserId;
    }

    // ── Create ───────────────────────────────────────────

    [Fact]
    public async Task CreateAccount_HashesPasswordAndActivatesRole()
    {
        var (success, message, user) = await _authService.CreateAccountAsync(
            "Skena Baru", "skena.baru@uji.test", "rahasia123", "Artist", "Bandung");

        Assert.True(success);
        Assert.NotNull(user);
        Assert.Equal("Artist", user!.Role);
        Assert.Equal("None", user.RoleStatus);          // langsung aktif, tanpa antrian verifikasi
        Assert.Equal("Bandung", user.City);

        var stored = await _context.Users.AsNoTracking().FirstAsync(u => u.Email == "skena.baru@uji.test");
        Assert.NotEqual("rahasia123", stored.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("rahasia123", stored.PasswordHash));
        Assert.Contains("Artist", message);
    }

    [Fact]
    public async Task CreateAccount_ArtistAndEo_GetEmptyProfile()
    {
        await _authService.CreateAccountAsync("Artis Uji", "artis@uji.test", "rahasia123", "Artist", "Jakarta");
        await _authService.CreateAccountAsync("Promotor Uji", "promotor@uji.test", "rahasia123", "EO", "Jakarta");

        Assert.True(await _context.Artists.AnyAsync(a => a.Name == "Artis Uji"));
        Assert.True(await _context.Organizers.AnyAsync(o => o.OrganizationName == "Promotor Uji"));
    }

    [Fact]
    public async Task CreateAccount_DuplicateEmail_Rejected()
    {
        await SeedUserIdAsync("sudah.ada@uji.test");

        var (success, message, user) = await _authService.CreateAccountAsync(
            "Skena Baru", "SUDAH.ADA@uji.test", "rahasia123", "User", "Jakarta");

        Assert.False(success);
        Assert.Null(user);
        Assert.Contains("sudah terdaftar", message);
    }

    [Theory]
    [InlineData("", "rahasia123", "User", "Nama wajib diisi")]
    [InlineData("Ab", "rahasia123", "User", "Nama wajib diisi")]
    [InlineData("Skena Baru", "12345", "User", "minimal 6 karakter")]
    [InlineData("Skena Baru", "rahasia123", "SuperAdmin", "Role tidak valid")]
    [InlineData("Skena Baru", "rahasia123", "Wizard", "Role tidak valid")]
    public async Task CreateAccount_InvalidInput_Rejected(string name, string password, string role, string expectedError)
    {
        var (success, message, user) = await _authService.CreateAccountAsync(
            name, "baru@uji.test", password, role, "Jakarta");

        Assert.False(success);
        Assert.Null(user);
        Assert.Contains(expectedError, message);
    }

    [Fact]
    public async Task CreateAccount_InvalidEmail_Rejected()
    {
        var (success, message, _) = await _authService.CreateAccountAsync(
            "Skena Baru", "bukan-email", "rahasia123", "User", "Jakarta");

        Assert.False(success);
        Assert.Contains("email tidak valid", message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Update ───────────────────────────────────────────

    [Fact]
    public async Task UpdateAccount_ChangesFieldsAndKeepsPasswordWhenEmpty()
    {
        var userId = await SeedUserIdAsync("ubah@uji.test");
        var originalHash = (await _context.Users.AsNoTracking().FirstAsync(u => u.UserId == userId)).PasswordHash;

        var (success, message) = await _authService.UpdateAccountAsync(
            userId, "Nama Baru", "ubah.baru@uji.test", password: null, role: "EO", city: "Surabaya");

        Assert.True(success);
        Assert.Equal("Akun diperbarui", message);

        var stored = await _context.Users.AsNoTracking().FirstAsync(u => u.UserId == userId);
        Assert.Equal("Nama Baru", stored.Name);
        Assert.Equal("ubah.baru@uji.test", stored.Email);
        Assert.Equal("EO", stored.Role);
        Assert.Equal("Surabaya", stored.City);
        Assert.Equal(originalHash, stored.PasswordHash);      // password tidak tersentuh
    }

    [Fact]
    public async Task UpdateAccount_WithPassword_RehashesPassword()
    {
        var userId = await SeedUserIdAsync("ganti.password@uji.test");
        var originalHash = (await _context.Users.AsNoTracking().FirstAsync(u => u.UserId == userId)).PasswordHash;

        await _authService.UpdateAccountAsync(userId, null, null, "passwordBaru123", null, null);

        var stored = await _context.Users.AsNoTracking().FirstAsync(u => u.UserId == userId);
        Assert.NotEqual(originalHash, stored.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("passwordBaru123", stored.PasswordHash));
    }

    [Fact]
    public async Task UpdateAccount_DuplicateEmailOfAnotherAccount_Rejected()
    {
        var userId = await SeedUserIdAsync("punya.a@uji.test");
        await SeedUserIdAsync("punya.b@uji.test");

        var (success, message) = await _authService.UpdateAccountAsync(
            userId, null, "punya.b@uji.test", null, null, null);

        Assert.False(success);
        Assert.Contains("sudah dipakai", message);
    }

    [Fact]
    public async Task UpdateAccount_MissingAccount_Rejected()
    {
        var (success, message) = await _authService.UpdateAccountAsync(9999, "X", null, null, null, null);

        Assert.False(success);
        Assert.Equal("Akun tidak ditemukan", message);
    }

    [Fact]
    public async Task UpdateAccount_RoleArtist_CreatesProfileOnce()
    {
        var userId = await SeedUserIdAsync("naik.kelas@uji.test");

        await _authService.UpdateAccountAsync(userId, null, null, null, "Artist", null);
        await _authService.UpdateAccountAsync(userId, null, null, null, "Artist", null);

        Assert.Equal(1, await _context.Artists.CountAsync(a => a.UserId == userId));
    }

    // ── Suspend ──────────────────────────────────────────

    [Fact]
    public async Task SuspendAccount_BlocksLoginAndCanBeRestored()
    {
        var targetId = await SeedUserIdAsync("ditangguhkan@uji.test");
        var adminId = await SeedUserIdAsync("admin.pelaku@uji.test", role: "Admin");

        var (suspended, suspendMessage) = await _authService.SetAccountSuspendedAsync(
            targetId, suspended: true, actingAdminId: adminId);

        Assert.True(suspended);
        Assert.Contains("dinonaktifkan", suspendMessage);

        var (loginOk, loginMessage, _, token) = await _authService.LoginAsync("ditangguhkan@uji.test", "rahasia123");
        Assert.False(loginOk);
        Assert.Null(token);
        Assert.Contains("dinonaktifkan", loginMessage);

        await _authService.SetAccountSuspendedAsync(targetId, suspended: false, actingAdminId: adminId);

        var (loginOkAgain, _, _, tokenAgain) = await _authService.LoginAsync("ditangguhkan@uji.test", "rahasia123");
        Assert.True(loginOkAgain);
        Assert.NotNull(tokenAgain);
    }

    [Fact]
    public async Task SuspendAccount_CannotSuspendSelf()
    {
        var adminId = await SeedUserIdAsync("admin.pelaku@uji.test", role: "Admin");

        var (success, message) = await _authService.SetAccountSuspendedAsync(
            adminId, suspended: true, actingAdminId: adminId);

        Assert.False(success);
        Assert.Contains("sendiri", message);
    }

    [Fact]
    public async Task SuspendAccount_MissingAccount_Rejected()
    {
        var (success, message) = await _authService.SetAccountSuspendedAsync(
            9999, suspended: true, actingAdminId: 1);

        Assert.False(success);
        Assert.Equal("Akun tidak ditemukan", message);
    }

    // ── Delete ───────────────────────────────────────────

    [Fact]
    public async Task DeleteAccount_RemovesAccountAndItsRelations()
    {
        var userId = await SeedUserIdAsync("hapus@uji.test");
        var adminId = await SeedUserIdAsync("admin.pelaku@uji.test", role: "Admin");
        _context.Follows.Add(new Follow { UserId = userId, ArtistId = 1 });
        _context.Favorites.Add(new Favorite { UserId = userId, EventId = 1 });
        _context.UserPreferences.Add(new UserPreference { UserId = userId, GenreId = 1 });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (success, message) = await _authService.DeleteAccountAsync(userId, actingAdminId: adminId);

        Assert.True(success);
        Assert.Contains("dihapus", message);
        Assert.False(await _context.Users.AnyAsync(u => u.UserId == userId));
        Assert.False(await _context.Follows.AnyAsync(f => f.UserId == userId));
        Assert.False(await _context.Favorites.AnyAsync(f => f.UserId == userId));
        Assert.False(await _context.UserPreferences.AnyAsync(p => p.UserId == userId));
    }

    [Fact]
    public async Task DeleteAccount_UnlinksArtistProfileInsteadOfDeletingWorks()
    {
        var userId = await SeedUserIdAsync("artis.dihapus@uji.test", role: "Artist");
        var adminId = await SeedUserIdAsync("admin.pelaku@uji.test", role: "Admin");
        var artist = new Artist { UserId = userId, Name = "Hollow Men Uji" };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        var artistId = artist.ArtistId;
        _context.ChangeTracker.Clear();

        await _authService.DeleteAccountAsync(userId, actingAdminId: adminId);

        var storedArtist = await _context.Artists.AsNoTracking().FirstAsync(a => a.ArtistId == artistId);
        Assert.Null(storedArtist.UserId);      // profil & karya tetap ada, hanya tautan akun dilepas
    }

    [Fact]
    public async Task DeleteAccount_CannotDeleteSelf()
    {
        var adminId = await SeedUserIdAsync("admin.pelaku@uji.test", role: "Admin");

        var (success, message) = await _authService.DeleteAccountAsync(adminId, actingAdminId: adminId);

        Assert.False(success);
        Assert.Contains("sendiri", message);
        Assert.True(await _context.Users.AnyAsync(u => u.UserId == adminId));
    }

    [Fact]
    public async Task DeleteAccount_CannotDeleteLastAdmin()
    {
        var adminId = await SeedUserIdAsync("admin.satu-satunya@uji.test", role: "Admin");

        var (success, message) = await _authService.DeleteAccountAsync(adminId, actingAdminId: 999);

        Assert.False(success);
        Assert.Contains("Admin terakhir", message);
        Assert.True(await _context.Users.AnyAsync(u => u.UserId == adminId));
    }

    [Fact]
    public async Task DeleteAccount_AdminCanBeDeletedWhenAnotherAdminRemains()
    {
        var firstAdminId = await SeedUserIdAsync("admin.pertama@uji.test", role: "Admin");
        var secondAdminId = await SeedUserIdAsync("admin.kedua@uji.test", role: "Admin");

        var (success, _) = await _authService.DeleteAccountAsync(firstAdminId, actingAdminId: secondAdminId);

        Assert.True(success);
        Assert.False(await _context.Users.AnyAsync(u => u.UserId == firstAdminId));
    }

    [Fact]
    public async Task DeleteAccount_MissingAccount_Rejected()
    {
        var (success, message) = await _authService.DeleteAccountAsync(9999, actingAdminId: 1);

        Assert.False(success);
        Assert.Equal("Akun tidak ditemukan", message);
    }
}
