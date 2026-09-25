using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Test CRUD data master Genre oleh Admin (§4.4, §23) — termasuk penolakan hapus
/// saat genre masih dipakai event atau preferensi user.
/// </summary>
public class GenreServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GenreService _service;

    public GenreServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new GenreService(_context);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private async Task<Genre> SeedGenreAsync(string name)
    {
        var genre = new Genre { Name = name, Icon = "🎸" };
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return genre;
    }

    // ── Read ─────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsGenresOrderedByName()
    {
        // Provider InMemory tidak menerapkan seed HasData, jadi data disiapkan sendiri.
        await SeedGenreAsync("Zouk");
        await SeedGenreAsync("Ambient");

        var genres = await _service.GetAllAsync();

        var names = genres.Select(g => g.Name).ToList();
        Assert.Equal("Ambient", names.First());
        Assert.Equal("Zouk", names.Last());
    }

    // ── Create ───────────────────────────────────────────

    [Fact]
    public async Task Create_TrimsAndPersists()
    {
        var (genre, error) = await _service.CreateAsync("  Shoegaze Baru  ", "🌙");

        Assert.Null(error);
        Assert.NotNull(genre);
        Assert.Equal("Shoegaze Baru", genre!.Name);
        Assert.Equal("🌙", genre.Icon);
        Assert.True(await _context.Genres.AnyAsync(g => g.GenreId == genre.GenreId));
    }

    [Fact]
    public async Task Create_DuplicateNameIgnoringCase_Rejected()
    {
        await SeedGenreAsync("Metalcore");

        var (genre, error) = await _service.CreateAsync("metalcore", "");

        Assert.Null(genre);
        Assert.Contains("sudah ada", error);
    }

    [Theory]
    [InlineData("", "Nama genre wajib diisi")]
    [InlineData("   ", "Nama genre wajib diisi")]
    public async Task Create_EmptyName_Rejected(string name, string expectedError)
    {
        var (genre, error) = await _service.CreateAsync(name, "");

        Assert.Null(genre);
        Assert.Equal(expectedError, error);
    }

    [Fact]
    public async Task Create_TooLongName_Rejected()
    {
        var (genre, error) = await _service.CreateAsync(new string('a', 51), "");

        Assert.Null(genre);
        Assert.Contains("maksimal 50", error);
    }

    // ── Update ───────────────────────────────────────────

    [Fact]
    public async Task Update_ChangesNameAndIcon()
    {
        var genre = await SeedGenreAsync("Post Rock");

        var (updated, error) = await _service.UpdateAsync(genre.GenreId, "Post-Rock", "🎛️");

        Assert.Null(error);
        Assert.Equal("Post-Rock", updated!.Name);
        Assert.Equal("🎛️", updated.Icon);
    }

    [Fact]
    public async Task Update_DuplicateNameOfAnotherGenre_Rejected()
    {
        var first = await SeedGenreAsync("Emo");
        await SeedGenreAsync("Screamo");

        var (updated, error) = await _service.UpdateAsync(first.GenreId, "Screamo", "");

        Assert.Null(updated);
        Assert.Contains("sudah ada", error);
    }

    [Fact]
    public async Task Update_KeepingOwnName_Allowed()
    {
        var genre = await SeedGenreAsync("Dream Pop");

        var (updated, error) = await _service.UpdateAsync(genre.GenreId, "Dream Pop", "☁️");

        Assert.Null(error);
        Assert.Equal("☁️", updated!.Icon);
    }

    [Fact]
    public async Task Update_MissingGenre_Rejected()
    {
        var (updated, error) = await _service.UpdateAsync(9999, "Apa Saja", "");

        Assert.Null(updated);
        Assert.Equal("Genre tidak ditemukan", error);
    }

    // ── Delete ───────────────────────────────────────────

    [Fact]
    public async Task Delete_UnusedGenre_Removed()
    {
        var genre = await SeedGenreAsync("Genre Tak Terpakai");

        var (success, error) = await _service.DeleteAsync(genre.GenreId);

        Assert.True(success);
        Assert.Null(error);
        Assert.False(await _context.Genres.AnyAsync(g => g.GenreId == genre.GenreId));
    }

    [Fact]
    public async Task Delete_UsedByEvent_Rejected()
    {
        var genre = await SeedGenreAsync("Genre Dipakai Event");
        var evt = TestData.MakeEvent(name: "Gig Pakai Genre");
        evt.GenreId = genre.GenreId;
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (success, error) = await _service.DeleteAsync(genre.GenreId);

        Assert.False(success);
        Assert.Contains("masih dipakai 1 event", error);
        Assert.True(await _context.Genres.AnyAsync(g => g.GenreId == genre.GenreId));
    }

    [Fact]
    public async Task Delete_UsedByUserPreference_Rejected()
    {
        var genre = await SeedGenreAsync("Genre Preferensi");
        _context.UserPreferences.Add(new UserPreference { UserId = 10, GenreId = genre.GenreId });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var (success, error) = await _service.DeleteAsync(genre.GenreId);

        Assert.False(success);
        Assert.Contains("preferensi", error);
        Assert.True(await _context.Genres.AnyAsync(g => g.GenreId == genre.GenreId));
    }

    [Fact]
    public async Task Delete_MissingGenre_Rejected()
    {
        var (success, error) = await _service.DeleteAsync(9999);

        Assert.False(success);
        Assert.Equal("Genre tidak ditemukan", error);
    }
}
