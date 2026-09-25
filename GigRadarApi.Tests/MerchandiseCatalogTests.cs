using System.Security.Claims;
using System.Text.Json;
using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test katalog publik merchandise (Bab 31, section Shop di ArtistDetailPage):
/// GET /api/artists/{artistId}/merchandise hanya menampilkan produk ACTIVE milik
/// artist itu sendiri, terurut terbaru, dan kosong bila belum ada produk.
/// </summary>
public class MerchandiseCatalogTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MerchandiseController _controller;

    private const int UserId = 80;
    private int _artistId;
    private int _otherArtistId;

    public MerchandiseCatalogTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var artist = new Artist { Name = "Hollow Men", UserId = UserId };
        var otherArtist = new Artist { Name = "Sunyi", UserId = UserId + 1 };
        _context.AddRange(artist, otherArtist);
        _context.SaveChanges();
        _artistId = artist.ArtistId;
        _otherArtistId = otherArtist.ArtistId;
        _context.ChangeTracker.Clear();

        _controller = new MerchandiseController(_context, new NotificationService(_context));
        SetUser(_controller, UserId, "Artist");
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private static void SetUser(ControllerBase controller, int userId, string role)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("UserId", userId.ToString()), new Claim("Role", role) }, "test"))
            }
        };
    }

    private static JsonElement[] Rows(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    private void SeedMerch(int artistId, string name, string status, DateTime createdAt)
    {
        _context.Merchandises.Add(new Merchandise
        {
            ArtistId = artistId,
            Name = name,
            Description = "Uji",
            Price = 100_000,
            Stock = 5,
            Status = status,
            CreatedAt = createdAt
        });
    }

    [Fact]
    public async Task GetForArtist_ReturnsOnlyActiveItemsOfThatArtist_NewestFirst()
    {
        var now = DateTime.UtcNow;
        SeedMerch(_artistId, "Kaos Lama", "ACTIVE", now.AddDays(-3));
        SeedMerch(_artistId, "Kaos Baru", "ACTIVE", now);
        SeedMerch(_artistId, "Kaos Arsip", "ARCHIVED", now);
        SeedMerch(_otherArtistId, "Milik Artist Lain", "ACTIVE", now);
        await _context.SaveChangesAsync();

        var result = await _controller.GetForArtist(_artistId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Rows(ok.Value);
        Assert.Equal(2, rows.Length);
        Assert.Equal("Kaos Baru", rows[0].GetProperty("Name").GetString());
        Assert.Equal("Kaos Lama", rows[1].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetForArtist_WithoutProducts_ReturnsEmptyList()
    {
        var result = await _controller.GetForArtist(_artistId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Rows(ok.Value));
    }
}
