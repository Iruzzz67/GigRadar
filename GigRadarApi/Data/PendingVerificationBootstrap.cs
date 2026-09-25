using GigRadarApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Data;

/// <summary>Melengkapi akun role-request lama agar masuk Verification Center.</summary>
public static class PendingVerificationBootstrap
{
    public static void Ensure(AppDbContext context)
    {
        var pendingRequests = context.RoleRequests
            .Include(r => r.User)
            .Where(r => r.Status == "Pending" && (r.RequestedRole == "Artist" || r.RequestedRole == "EO"))
            .ToList();

        foreach (var request in pendingRequests)
        {
            var user = request.User;
            if (user == null) continue;

            if (request.RequestedRole == "Artist")
            {
                var artist = context.Artists.FirstOrDefault(a => a.UserId == user.UserId);
                if (artist == null)
                {
                    artist = new Artist
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        VerificationStatus = "PENDING",
                        IsVerified = false
                    };
                    context.Artists.Add(artist);
                    context.SaveChanges();
                }

                if (!context.ArtistVerifications.Any(v => v.ArtistId == artist.ArtistId && v.Status == "PENDING"))
                {
                    context.ArtistVerifications.Add(new ArtistVerification
                    {
                        ArtistId = artist.ArtistId,
                        LegalName = user.Name,
                        SupportingInfo = "Pengajuan akun Artist dari proses registrasi.",
                        Status = "PENDING",
                        SubmittedAt = request.CreatedAt
                    });
                }
            }
            else
            {
                var organizer = context.Organizers.FirstOrDefault(o => o.UserId == user.UserId);
                if (organizer == null)
                {
                    organizer = new Organizer
                    {
                        UserId = user.UserId,
                        OrganizationName = user.Name,
                        Location = user.City,
                        Description = "Pengajuan akun Event Organizer dari proses registrasi.",
                        VerificationStatus = "PENDING",
                        IsVerified = false
                    };
                    context.Organizers.Add(organizer);
                    context.SaveChanges();
                }

                if (!context.OrganizerVerifications.Any(v => v.OrganizerId == organizer.OrganizerId && v.Status == "PENDING"))
                {
                    context.OrganizerVerifications.Add(new OrganizerVerification
                    {
                        OrganizerId = organizer.OrganizerId,
                        OrganizationName = organizer.OrganizationName,
                        Description = organizer.Description,
                        Location = organizer.Location,
                        Status = "PENDING",
                        SubmittedAt = request.CreatedAt
                    });
                }
            }

            user.RoleStatus = "Pending";
        }

        context.SaveChanges();
    }
}
