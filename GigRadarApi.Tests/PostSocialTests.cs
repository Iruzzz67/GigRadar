using System.Security.Claims;
using GigRadarApi.Controllers;
using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test interaksi sosial post (GIGRADAR_APP_SPEC.md Bab 32, 48):
/// toggle like idempoten, komentar wajib berisi & terbatas panjang,
/// dan post yang tidak dipublikasikan tidak menerima komentar.
/// </summary>
public class PostSocialTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PostsController _controller;

    // Id user seed: 1 = Admin, 2 = EO. Test pakai id lain.
    private const int UserId = 30;

    public PostSocialTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _controller = new PostsController(_context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim("UserId", UserId.ToString()) }, "test"))
                }
            }
        };

        Seed();
    }

    private void Seed()
    {
        var artist = new Artist { Name = "Hollow Men", Genre = "Shoegaze" };
        _context.Artists.Add(artist);
        _context.ArtistPosts.Add(new ArtistPost
        {
            ArtistId = artist.ArtistId,
            Title = "Single baru",
            Content = "Afterglow sudah rilis!",
            IsPublished = true
        });
        _context.ArtistPosts.Add(new ArtistPost
        {
            ArtistId = artist.ArtistId,
            Title = "Draft",
            Content = "Belum tayang",
            IsPublished = false
        });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private ArtistPost PublishedPost => _context.ArtistPosts.Single(p => p.IsPublished);
    private ArtistPost DraftPost => _context.ArtistPosts.Single(p => !p.IsPublished);

    public void Dispose() => _context.Database.EnsureDeleted();

    // ── Like ─────────────────────────────────────────────

    [Fact]
    public async Task ToggleLike_FirstLike_ReturnsLikedWithCount()
    {
        var result = await _controller.ToggleLike(PublishedPost.PostId);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)ok.Value!.GetType().GetProperty("liked")!.GetValue(ok.Value)!);
        Assert.Equal(1, (int)ok.Value.GetType().GetProperty("likesCount")!.GetValue(ok.Value)!);
    }

    [Fact]
    public async Task ToggleLike_SecondCall_Unlikes_Idempotent()
    {
        await _controller.ToggleLike(PublishedPost.PostId);
        var result = await _controller.ToggleLike(PublishedPost.PostId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.False((bool)ok.Value!.GetType().GetProperty("liked")!.GetValue(ok.Value)!);
        Assert.Equal(0, (int)ok.Value.GetType().GetProperty("likesCount")!.GetValue(ok.Value)!);
        Assert.Empty(await _context.PostLikes.ToListAsync());
    }

    [Fact]
    public async Task ToggleLike_UnknownPost_Returns404()
    {
        var result = await _controller.ToggleLike(9999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ── Komentar ─────────────────────────────────────────

    [Fact]
    public async Task AddComment_Valid_ReturnsCommentWithUserName()
    {
        var result = await _controller.AddComment(PublishedPost.PostId, new AddCommentRequest
        {
            Content = "Keren banget!"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        Assert.Contains("Keren banget!", json);
    }

    [Fact]
    public async Task AddComment_Empty_Returns400()
    {
        var result = await _controller.AddComment(PublishedPost.PostId, new AddCommentRequest { Content = "   " });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddComment_TooLong_Returns400()
    {
        var result = await _controller.AddComment(PublishedPost.PostId,
            new AddCommentRequest { Content = new string('x', 1001) });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddComment_OnDraftPost_Returns400()
    {
        var result = await _controller.AddComment(DraftPost.PostId, new AddCommentRequest { Content = "hai" });
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddComment_UnknownPost_Returns404()
    {
        var result = await _controller.AddComment(9999, new AddCommentRequest { Content = "hai" });
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetComments_ReturnsOrderedWithUserName()
    {
        _context.PostComments.Add(new PostComment
        {
            PostId = PublishedPost.PostId,
            UserId = 2,
            Content = "Pertamax",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        _context.PostComments.Add(new PostComment
        {
            PostId = PublishedPost.PostId,
            UserId = 1,
            Content = "Mantap",
            CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User { UserId = 1, Name = "Admin", Email = "admin2@x.com" });
        _context.Users.Add(new User { UserId = 2, Name = "Event Organizer", Email = "eo2@x.com" });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var result = await _controller.GetComments(PublishedPost.PostId);
        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        Assert.Contains("Pertamax", json);
        Assert.Contains("Mantap", json);
        // Urutan terlama dulu: "Pertamax" harus muncul sebelum "Mantap".
        Assert.True(json.IndexOf("Pertamax", StringComparison.Ordinal) < json.IndexOf("Mantap", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetComments_UnknownPost_Returns404()
    {
        var result = await _controller.GetComments(9999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
