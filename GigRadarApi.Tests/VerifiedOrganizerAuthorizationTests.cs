using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GigRadarApi.Authorization;
using GigRadarApi.Data;
using GigRadarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GigRadarApi.Tests;

/// <summary>
/// Unit test gate anti-penipuan event (§53/§54): membuat/mengubah event dan mengelola
/// tiket hanya boleh oleh EO yang organizer-nya sudah diverifikasi Admin. Fokus:
/// Admin tidak terblokir, EO terverifikasi lolos, EO belum terverifikasi/belum punya
/// profil ditolak dengan kode yang bisa dibaca klien, dan isolasi antar-EO dijaga.
/// </summary>
public class VerifiedOrganizerAuthorizationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VerifiedOrganizerHandler _handler;

    private const int AdminId = 1;
    private const int EoUserId = 20;
    private const int OtherEoUserId = 21;

    public VerifiedOrganizerAuthorizationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _handler = new VerifiedOrganizerHandler(_context);
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private static ClaimsPrincipal Principal(string? role, int? userId = null)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (userId != null) claims.Add(new Claim("UserId", userId.Value.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "TestAuth"));
    }

    private async Task<AuthorizationHandlerContext> HandleAsync(ClaimsPrincipal user)
    {
        var context = new AuthorizationHandlerContext([new VerifiedOrganizerRequirement()], user, resource: null);
        await _handler.HandleAsync(context);
        return context;
    }

    private static string[] Failures(AuthorizationHandlerContext context) =>
        context.FailureReasons.Select(reason => reason.Message).ToArray();

    private async Task<Organizer> SeedOrganizerAsync(int userId, bool isVerified)
    {
        var organizer = new Organizer
        {
            UserId = userId,
            OrganizationName = "Promotor Uji",
            Description = "Promotor gig indie.",
            Location = "Jakarta",
            Contact = "081234567890",
            VerificationStatus = isVerified ? "VERIFIED" : "NOT_SUBMITTED",
            IsVerified = isVerified
        };
        _context.Organizers.Add(organizer);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return organizer;
    }

    // ── Keputusan policy ─────────────────────────────────

    [Fact]
    public async Task Admin_Allowed_WithoutOrganizerProfile()
    {
        var context = await HandleAsync(Principal("Admin", AdminId));

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task VerifiedOrganizer_Allowed()
    {
        await SeedOrganizerAsync(EoUserId, isVerified: true);

        var context = await HandleAsync(Principal("EO", EoUserId));

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task UnverifiedOrganizer_DeniedWithActionableCode()
    {
        await SeedOrganizerAsync(EoUserId, isVerified: false);

        var context = await HandleAsync(Principal("EO", EoUserId));

        Assert.False(context.HasSucceeded);
        Assert.Contains(VerifiedOrganizerHandler.OrganizerNotVerifiedCode, Failures(context));
    }

    [Fact]
    public async Task OrganizerWithoutProfile_Denied()
    {
        var context = await HandleAsync(Principal("EO", EoUserId));

        Assert.False(context.HasSucceeded);
        Assert.Contains(VerifiedOrganizerHandler.OrganizerNotVerifiedCode, Failures(context));
    }

    [Fact]
    public async Task EoWithoutUserIdClaim_Denied()
    {
        var context = await HandleAsync(Principal("EO"));

        Assert.False(context.HasSucceeded);
        Assert.Contains(VerifiedOrganizerHandler.OrganizerNotVerifiedCode, Failures(context));
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Artist")]
    [InlineData(null)]
    public async Task NonEoRole_DeniedWithRoleCode(string? role)
    {
        var context = await HandleAsync(Principal(role, EoUserId));

        Assert.False(context.HasSucceeded);
        Assert.Contains(VerifiedOrganizerHandler.NotOrganizerCode, Failures(context));
    }

    [Fact]
    public async Task VerifiedOrganizerOfAnotherUser_DoesNotGrantAccess()
    {
        await SeedOrganizerAsync(OtherEoUserId, isVerified: true);

        var context = await HandleAsync(Principal("EO", EoUserId));

        Assert.False(context.HasSucceeded);
        Assert.Contains(VerifiedOrganizerHandler.OrganizerNotVerifiedCode, Failures(context));
    }

    // ── Kontrak respons 403 ──────────────────────────────

    [Fact]
    public async Task Forbidden_ReturnsJsonWithReasonAndGuidance()
    {
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        httpContext.Request.Path = "/api/events";

        var failure = AuthorizationFailure.Failed(
            [new AuthorizationFailureReason(_handler, VerifiedOrganizerHandler.OrganizerNotVerifiedCode)]);
        var resultHandler = new ApiAuthorizationResultHandler();

        await resultHandler.HandleAsync(
            _ => Task.CompletedTask,
            httpContext,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(),
            PolicyAuthorizationResult.Forbid(failure));

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        var body = await new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEndAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal(VerifiedOrganizerHandler.OrganizerNotVerifiedCode,
            json.RootElement.GetProperty("code").GetString());
        Assert.Contains("belum terverifikasi", json.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public void WriteEndpoints_RequireVerifiedOrganizerPolicy()
    {
        var expected = new[]
        {
            (typeof(GigRadarApi.Controllers.EventsController), nameof(GigRadarApi.Controllers.EventsController.CreateEvent)),
            (typeof(GigRadarApi.Controllers.EventsController), nameof(GigRadarApi.Controllers.EventsController.UpdateEvent)),
            (typeof(GigRadarApi.Controllers.EventsController), nameof(GigRadarApi.Controllers.EventsController.ChangeEventStatus)),
            (typeof(GigRadarApi.Controllers.EventsController), nameof(GigRadarApi.Controllers.EventsController.DeleteEvent)),
            (typeof(GigRadarApi.Controllers.TicketsController), nameof(GigRadarApi.Controllers.TicketsController.AddTicketType)),
            (typeof(GigRadarApi.Controllers.TicketsController), nameof(GigRadarApi.Controllers.TicketsController.UpdateTicketType)),
            (typeof(GigRadarApi.Controllers.TicketsController), nameof(GigRadarApi.Controllers.TicketsController.DeleteTicketType)),
        };

        foreach (var (controller, actionName) in expected)
        {
            var action = controller.GetMethod(actionName)!;
            var authorize = action.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .Single();

            Assert.Equal("VerifiedOrganizer", authorize.Policy);
            Assert.Equal("EO,Admin", authorize.Roles);
        }
    }
}
