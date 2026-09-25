using System.Security.Claims;
using System.Text.Json;
using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test fitur Community halaman artist (CommunityController): daftar post
/// hanya milik artist yang diminta dan terurut terbaru, post baru selalu tercatat
/// sebagai user yang login (bukan angka yang dikirim body), dan artist yang tidak
/// ada ditolak tanpa menyisipkan data.
/// </summary>
public class CommunityPostTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CommunityController _controller;

    private const int UserId = 60;
    private const int OtherUserId = 61;
    private int _artistId;
    private int _otherArtistId;

    public CommunityPostTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var me = new User { UserId = UserId, Name = "Penyuji", Email = "penyuji@uji.test" };
        var other = new User { UserId = OtherUserId, Name = "Orang Lain", Email = "lain@uji.test" };
        var artist = new Artist { Name = "Hollow Men", UserId = UserId };
        var otherArtist = new Artist { Name = "Sunyi", UserId = OtherUserId };
        _context.AddRange(me, other, artist, otherArtist);
        _context.SaveChanges();
        _artistId = artist.ArtistId;
        _otherArtistId = otherArtist.ArtistId;
        _context.ChangeTracker.Clear();

        _controller = new CommunityController(_context);
        SetUser(_controller, UserId);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private static void SetUser(ControllerBase controller, int userId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("UserId", userId.ToString()), new Claim("Role", "User") }, "test"))
            }
        };
    }

    /// <summary>Baris hasil GET sebagai JSON — casing properti mengikuti controller.</summary>
    private static JsonElement[] Rows(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    [Fact]
    public async Task Get_ReturnsOnlyThatArtistPosts_NewestFirst()
    {
        _context.CommunityPosts.AddRange(
            new CommunityPost { ArtistId = _artistId, UserId = UserId, Content = "Lama",
                CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new CommunityPost { ArtistId = _artistId, UserId = OtherUserId, Content = "Baru",
                CreatedAt = DateTime.UtcNow },
            new CommunityPost { ArtistId = _otherArtistId, UserId = OtherUserId, Content = "Milik artist lain",
                CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _controller.GetCommunityPosts(_artistId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Rows(ok.Value);
        Assert.Equal(2, rows.Length);
        Assert.Equal("Baru", rows[0].GetProperty("Content").GetString());
        Assert.Equal("Orang Lain", rows[0].GetProperty("UserName").GetString());
        Assert.Equal("Penyuji", rows[1].GetProperty("UserName").GetString());
    }

    [Fact]
    public async Task Get_UnknownArtist_ReturnsEmptyListNotEmpty404()
    {
        var result = await _controller.GetCommunityPosts(999999);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Rows(ok.Value));
    }

    [Fact]
    public async Task Create_UsesCurrentUserFromClaim_IgnoresUserIdInBody()
    {
        var result = await _controller.CreatePost(_artistId,
            new CommunityPost { Content = "Semangat!", UserId = OtherUserId });

        Assert.IsType<CreatedAtActionResult>(result);
        var saved = Assert.Single(_context.CommunityPosts);
        Assert.Equal("Semangat!", saved.Content);
        Assert.Equal(UserId, saved.UserId);
        Assert.Equal(_artistId, saved.ArtistId);
    }

    [Fact]
    public async Task Create_UnknownArtist_ReturnsNotFoundAndSavesNothing()
    {
        var result = await _controller.CreatePost(999999, new CommunityPost { Content = "Halo" });

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, await _context.CommunityPosts.CountAsync());
    }
}
