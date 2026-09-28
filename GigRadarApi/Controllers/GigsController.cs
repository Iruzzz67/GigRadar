using GigRadarApi.Models;
using GigRadarApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GigRadarApi.Controllers;

/// <summary>
/// Gig marketplace (dokumen research §5/§6):
/// - EO mengelola slot artist pada event miliknya (CRUD + lifecycle DRAFT/OPEN/CLOSED).
/// - Artist menemukan gig terbuka dan melamar.
/// - EO meninjau lamaran, shortlist, atau menolak.
/// Semua kepemilikan diverifikasi di service layer, bukan dari klaim client.
/// </summary>
[Route("api")]
[ApiController]
[Authorize]
public class GigsController(GigService gigs, GigApplicationService applications) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst("UserId")!.Value);
    private string CurrentRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        ?? User.FindFirst("Role")?.Value
        ?? "User";

    // ── EO: kelola gig pada event miliknya ──────────────

    [HttpGet("events/{eventId:int}/gigs")]
    [Authorize(Roles = "EO,Admin")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var (items, error, forbidden) =
            await gigs.GetForEventAsync(eventId, CurrentUserId, CurrentRole);
        if (forbidden) return Forbid();
        if (items == null) return NotFound(new { message = error });

        return Ok(items.Select(gig => GigResponse.From(gig, gig.Applications.Count)));
    }

    [HttpPost("events/{eventId:int}/gigs")]
    [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateGigRequest request)
    {
        var (gig, error, forbidden, notFound) =
            await gigs.CreateAsync(eventId, CurrentUserId, CurrentRole, request);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (gig == null) return BadRequest(new { message = error });

        return StatusCode(201, new { message = "Gig berhasil dibuat", gig = GigResponse.From(gig) });
    }

    [HttpPut("events/{eventId:int}/gigs/{gigId:int}")]
    [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
    public async Task<IActionResult> Update(int eventId, int gigId, [FromBody] CreateGigRequest request)
    {
        var (gig, error, forbidden, notFound) =
            await gigs.UpdateAsync(eventId, gigId, CurrentUserId, CurrentRole, request);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (gig == null) return BadRequest(new { message = error });

        return Ok(new { message = "Gig diperbarui", gig = GigResponse.From(gig) });
    }

    [HttpPost("events/{eventId:int}/gigs/{gigId:int}/status")]
    [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
    public async Task<IActionResult> ChangeStatus(int eventId, int gigId, [FromBody] ChangeGigStatusRequest request)
    {
        var (gig, error, forbidden, notFound) =
            await gigs.ChangeStatusAsync(eventId, gigId, CurrentUserId, CurrentRole, request.Status);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (gig == null) return BadRequest(new { message = error });

        return Ok(new { message = $"Status gig menjadi {gig.Status}", gig = GigResponse.From(gig) });
    }

    [HttpDelete("events/{eventId:int}/gigs/{gigId:int}")]
    [Authorize(Roles = "EO,Admin", Policy = "VerifiedOrganizer")]
    public async Task<IActionResult> Delete(int eventId, int gigId)
    {
        var (success, message, forbidden, notFound) =
            await gigs.DeleteAsync(eventId, gigId, CurrentUserId, CurrentRole);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message });
        if (!success) return BadRequest(new { message });

        return Ok(new { message });
    }

    // ── Artist: penemuan gig terbuka ────────────────────

    /// <summary>Daftar gig OPEN untuk Discover artist (filter q/genreId/maxBudget/city).</summary>
    [HttpGet("gigs")]
    public async Task<IActionResult> Discover(
        [FromQuery] string? q, [FromQuery] int? genreId,
        [FromQuery] decimal? maxBudget, [FromQuery] string? city)
    {
        var items = await gigs.GetOpenGigsAsync(CurrentUserId, q, genreId, maxBudget, city);
        return Ok(items);
    }

    // ── Artist: apply & withdraw ────────────────────────

    [HttpPost("gigs/{gigId:int}/applications")]
    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> Apply(int gigId, [FromBody] ApplyToGigRequest request)
    {
        var (application, error, forbidden, notFound) =
            await applications.ApplyAsync(gigId, CurrentUserId, request);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (application == null) return BadRequest(new { message = error });

        return StatusCode(201, new
        {
            message = "Lamaran terkirim",
            application = ToApplicationResponse(application)
        });
    }

    [HttpGet("applications/me")]
    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> GetMyApplications()
    {
        var items = await applications.GetMyAsync(CurrentUserId);
        return Ok(items.Select(a => ToApplicationResponse(a, includeGig: true)));
    }

    [HttpPost("applications/{applicationId:int}/withdraw")]
    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> Withdraw(int applicationId)
    {
        var (success, message, forbidden, notFound) =
            await applications.WithdrawAsync(applicationId, CurrentUserId);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message });
        if (!success) return BadRequest(new { message });

        return Ok(new { message });
    }

    // ── EO: meninjau lamaran ────────────────────────────

    [HttpGet("gigs/{gigId:int}/applications")]
    [Authorize(Roles = "EO,Admin")]
    public async Task<IActionResult> GetApplicationsForGig(int gigId)
    {
        var (items, error, forbidden, notFound) =
            await applications.GetForGigAsync(gigId, CurrentUserId, CurrentRole);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });

        return Ok(items!.Select(a => ToApplicationResponse(a)));
    }

    [HttpPost("applications/{applicationId:int}/shortlist")]
    [Authorize(Roles = "EO,Admin")]
    public async Task<IActionResult> Shortlist(int applicationId)
    {
        var (application, error, forbidden, notFound) =
            await applications.DecideAsync(applicationId, CurrentUserId, CurrentRole, shortlist: true);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (application == null) return BadRequest(new { message = error });

        return Ok(new { message = "Lamaran di-shortlist" });
    }

    [HttpPost("applications/{applicationId:int}/reject")]
    [Authorize(Roles = "EO,Admin")]
    public async Task<IActionResult> Reject(int applicationId)
    {
        var (application, error, forbidden, notFound) =
            await applications.DecideAsync(applicationId, CurrentUserId, CurrentRole, shortlist: false);
        if (forbidden) return Forbid();
        if (notFound) return NotFound(new { message = error });
        if (application == null) return BadRequest(new { message = error });

        return Ok(new { message = "Lamaran ditolak" });
    }

    // ── Response shaping ────────────────────────────────

    private static GigApplicationResponse ToApplicationResponse(GigApplication application, bool includeGig = false) =>
        GigApplicationResponse.From(application, includeGig);
}

/// <summary>Kontrak response lamaran gig (dipakai artist & EO).</summary>
public class GigApplicationResponse
{
    public int GigApplicationId { get; set; }
    public int GigId { get; set; }
    public int ArtistId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string ArtistGenre { get; set; } = string.Empty;
    public string ArtistCity { get; set; } = string.Empty;
    public string GigName { get; set; } = string.Empty;
    public int? EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    public static GigApplicationResponse From(GigApplication application, bool includeGig = false) => new()
    {
        GigApplicationId = application.GigApplicationId,
        GigId = application.GigId,
        ArtistId = application.ArtistId,
        Message = application.Message,
        Status = application.Status,
        CreatedAt = application.CreatedAt,
        UpdatedAt = application.UpdatedAt,
        RespondedAt = application.RespondedAt,
        ArtistName = application.Artist?.Name ?? string.Empty,
        ArtistGenre = application.Artist?.Genre ?? string.Empty,
        ArtistCity = application.Artist?.City ?? string.Empty,
        GigName = includeGig ? application.Gig?.Name ?? string.Empty : string.Empty,
        EventId = includeGig ? application.Gig?.EventId : null,
        EventName = includeGig ? application.Gig?.Event?.Name ?? string.Empty : string.Empty
    };
}

public class ChangeGigStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
