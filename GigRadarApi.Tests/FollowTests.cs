using System.Security.Claims;
using System.Text.Json;
using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test follow artist (§30, endpoint /api/users/follows): idempoten saat diikuti
/// berulang, isolasi antar-user pada daftar follows, unfollow yang aman dipanggil
/// dua kali, dan artist yang tidak ada ditolak.
/// </summary>
public class FollowTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UsersController _controller;

    private const int UserId = 70;
    private const int OtherUserId = 71;
    private int _artistId;
    private int _otherArtistId;

    public FollowTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var notifications = new NotificationService(_context);
        var controller = new UsersController(
            _context,
            new AuthService(_context, Configuration()),
            new ArtistVerificationService(_context, notifications),
            new OrganizerVerificationService(_context, notifications),
            new EventService(_context));
        _controller = controller;

        var me = new User { UserId = UserId, Name = "Penyuji", Email = "penyuji@uji.test" };
        var other = new User { UserId = OtherUserId, Name = "Orang Lain", Email = "lain@uji.test" };
        var artist = new Artist { Name = "Hollow Men", UserId = UserId };
        var otherArtist = new Artist { Name = "Sunyi", UserId = OtherUserId };
        _context.AddRange(me, other, artist, otherArtist);
        _context.SaveChanges();
        _artistId = artist.ArtistId;
        _otherArtistId = otherArtist.ArtistId;
        _context.ChangeTracker.Clear();

        SetUser(_controller, UserId);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private static void SetUser(ControllerBase controller, int userId, string role = "User")
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

    private static IConfiguration Configuration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "kunci-uji-yang-cukup-panjang-untuk-hmac-256-bit",
            ["JwtSettings:Issuer"] = "GigRadarTest",
            ["JwtSettings:Audience"] = "GigRadarTest",
            ["JwtSettings:ExpirationMinutes"] = "60"
        }).Build();

    private static JsonElement[] Rows(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    private static bool Flag(object? value, string name)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty(name).GetBoolean();
    }

    [Fact]
    public async Task Follow_SameArtistTwice_KeepsSingleRow()
    {
        var first = await _controller.FollowArtist(_artistId);
        var second = await _controller.FollowArtist(_artistId);

        Assert.True(Flag(Assert.IsType<OkObjectResult>(first).Value, "followed"));
        Assert.True(Flag(Assert.IsType<OkObjectResult>(second).Value, "followed"));
        Assert.Equal(1, await _context.Follows.CountAsync(f => f.ArtistId == _artistId));
    }

    [Fact]
    public async Task Follow_UnknownArtist_ReturnsNotFoundAndSavesNothing()
    {
        var result = await _controller.FollowArtist(999999);

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, await _context.Follows.CountAsync());
    }

    [Fact]
    public async Task GetFollows_ReturnsOnlyMyArtists()
    {
        _context.Follows.AddRange(
            new Follow { UserId = UserId, ArtistId = _artistId },
            new Follow { UserId = UserId, ArtistId = _otherArtistId },
            new Follow { UserId = OtherUserId, ArtistId = _artistId });
        await _context.SaveChangesAsync();

        var result = await _controller.GetFollows();

        var ok = Assert.IsType<OkObjectResult>(result);
        var rows = Rows(ok.Value);
        Assert.Equal(2, rows.Length);
        var json = JsonSerializer.Serialize(ok.Value);
        Assert.DoesNotContain("Orang Lain", json);
    }

    [Fact]
    public async Task Unfollow_RemovesOnlyMyFollow()
    {
        _context.Follows.AddRange(
            new Follow { UserId = UserId, ArtistId = _artistId },
            new Follow { UserId = OtherUserId, ArtistId = _artistId });
        await _context.SaveChangesAsync();

        var result = await _controller.UnfollowArtist(_artistId);

        Assert.False(Flag(Assert.IsType<OkObjectResult>(result).Value, "followed"));
        var remaining = Assert.Single(_context.Follows);
        Assert.Equal(OtherUserId, remaining.UserId);
    }

    [Fact]
    public async Task Unfollow_WhenNotFollowing_StillReturnsOk()
    {
        var result = await _controller.UnfollowArtist(_artistId);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(0, await _context.Follows.CountAsync());
    }
}
