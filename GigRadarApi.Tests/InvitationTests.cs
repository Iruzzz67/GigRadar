using System.Security.Claims;
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
/// Unit test undangan artist ↔ EO (GIGRADAR_APP_SPEC.md Bab 25–26):
/// ownership event, satu undangan PENDING, accept → line-up, reject,
/// cancel oleh EO, dan proteksi respond oleh artist yang salah.
/// </summary>
public class InvitationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly InvitationsController _eoController;
    private readonly InvitationsController _artistController;
    private readonly InvitationsController _otherArtistController;

    private const int EoUserId = 2;
    private const int ArtistUserId = 40;
    private const int OtherArtistUserId = 41;
    private int _eventId;
    private int _artistId;
    private int _otherArtistId;

    public InvitationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        var notifications = new NotificationService(_context);
        _eoController = new InvitationsController(_context, notifications);
        _artistController = new InvitationsController(_context, notifications);
        _otherArtistController = new InvitationsController(_context, notifications);

        Seed();
    }

    private void Seed()
    {
        var eo = new User { UserId = EoUserId, Name = "EO", Email = "eo@x.com" };
        var artistUser = new User { UserId = ArtistUserId, Name = "Artist", Email = "a@x.com" };
        var otherUser = new User { UserId = OtherArtistUserId, Name = "Other", Email = "o@x.com" };
        var venue = new Venue { Name = "V", City = "Jakarta" };
        var ev = new Event { Name = "Night of Shoegaze", VenueId = venue.VenueId, CreatedBy = EoUserId, Status = "Published" };
        var artist = new Artist { Name = "Hollow Men", UserId = ArtistUserId };
        var otherArtist = new Artist { Name = "Other Band", UserId = OtherArtistUserId };
        _context.AddRange(eo, artistUser, otherUser, venue, ev, artist, otherArtist);
        _context.SaveChanges();
        _eventId = ev.EventId;
        _artistId = artist.ArtistId;
        _otherArtistId = otherArtist.ArtistId;
        _context.ChangeTracker.Clear();

        // Controllers harus memakai context yang sama dengan seed — buat ulang dengan options utama.
        _eoController.ControllerContext = MakeContext(EoUserId, "EO");
        _artistController.ControllerContext = MakeContext(ArtistUserId, "Artist");
        _otherArtistController.ControllerContext = MakeContext(OtherArtistUserId, "Artist");
        ReplaceControllerContexts();
    }

    private ControllerContext MakeContext(int userId, string role) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim("UserId", userId.ToString()), new Claim("Role", role) }, "test"))
        }
    };

    private void ReplaceControllerContexts()
    {
        // Pastikan semua controller membaca/menulis ke _context (satu InMemory DB instance).
        SetContext(_eoController);
        SetContext(_artistController);
        SetContext(_otherArtistController);
    }

    private void SetContext(InvitationsController controller)
    {
        var field = typeof(InvitationsController).GetField("_context",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field!.SetValue(controller, _context);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    [Fact]
    public async Task Invite_Valid_ReturnsPendingInvitation()
    {
        var result = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest
        {
            ArtistId = _artistId,
            Message = "Mau main di event kami?"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("terkirim", System.Text.Json.JsonSerializer.Serialize(ok.Value));
    }

    [Fact]
    public async Task Invite_ByOtherEo_ReturnsForbidden()
    {
        var result = await _otherArtistController.InviteArtist(_eventId,
            new CreateInvitationRequest { ArtistId = _artistId });

        // User lain (bukan pemilik event) ditolak — baik role Artist maupun EO asing.
        Assert.True(result is ForbidResult or BadRequestObjectResult or NotFoundObjectResult);
    }

    [Fact]
    public async Task Invite_DuplicatePending_Rejected()
    {
        await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var result = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(await _context.EventInvitations.ToListAsync());
    }

    [Fact]
    public async Task Invite_ArtistAlreadyInLineup_Rejected()
    {
        _context.EventArtists.Add(new EventArtist { EventId = _eventId, ArtistId = _artistId, Order = 1 });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Accept_ByInvitedArtist_AddsToLineup()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);

        var result = await _artistController.AcceptInvitation(invitationId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("line-up", System.Text.Json.JsonSerializer.Serialize(ok.Value));
        Assert.True(await _context.EventArtists.AnyAsync(ea => ea.EventId == _eventId && ea.ArtistId == _artistId));

        var invitation = await _context.EventInvitations.AsNoTracking().SingleAsync();
        Assert.Equal("ACCEPTED", invitation.Status);
        Assert.NotNull(invitation.RespondedAt);
    }

    [Fact]
    public async Task Accept_ByWrongArtist_Fails()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);

        var result = await _otherArtistController.AcceptInvitation(invitationId);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(await _context.EventArtists.AnyAsync(ea => ea.ArtistId == _otherArtistId));
    }

    [Fact]
    public async Task Reject_SetsRejectedStatus()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);

        var result = await _artistController.RejectInvitation(invitationId);

        Assert.IsType<OkObjectResult>(result);
        var invitation = await _context.EventInvitations.AsNoTracking().SingleAsync();
        Assert.Equal("REJECTED", invitation.Status);
        Assert.False(await _context.EventArtists.AnyAsync(ea => ea.EventId == _eventId && ea.ArtistId == _artistId));
    }

    [Fact]
    public async Task Cancel_PendingInvitation_SetsCancelled()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);

        var result = await _eoController.CancelInvitation(invitationId);

        Assert.IsType<OkObjectResult>(result);
        var invitation = await _context.EventInvitations.AsNoTracking().SingleAsync();
        Assert.Equal("CANCELLED", invitation.Status);
    }

    [Fact]
    public async Task Cancel_AfterAccepted_Fails()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);
        await _artistController.AcceptInvitation(invitationId);

        var result = await _eoController.CancelInvitation(invitationId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Respond_Twice_Fails()
    {
        var create = await _eoController.InviteArtist(_eventId, new CreateInvitationRequest { ArtistId = _artistId });
        var invitationId = ExtractId(create);
        await _artistController.AcceptInvitation(invitationId);

        var result = await _artistController.RejectInvitation(invitationId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static int ExtractId(IActionResult result)
    {
        var ok = Assert.IsType<OkObjectResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        // Anonymous type mempertahankan PascalCase ("InvitationId") — coba kedua varian.
        return root.TryGetProperty("invitationId", out var camel) ? camel.GetInt32()
             : root.GetProperty("InvitationId").GetInt32();
    }
}
