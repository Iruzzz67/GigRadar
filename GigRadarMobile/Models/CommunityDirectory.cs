namespace GigRadarMobile.Models;

public class CommunityDirectory
{
    public List<CommunityUser> Users { get; set; } = new();
    public List<CommunityArtist> Artists { get; set; } = new();
    public List<CommunityOrganizer> Eos { get; set; } = new();
}

public class CommunityUser
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string JoinedLabel => $"Bergabung {CreatedAt.ToLocalTime():dd MMM yyyy}";
}

public class CommunityArtist
{
    public int ArtistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string AccountEmail { get; set; } = string.Empty;
    public string VerificationLabel => IsVerified ? "Terverifikasi" : "Belum terverifikasi";
}

public class CommunityOrganizer
{
    public int OrganizerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string AccountEmail { get; set; } = string.Empty;
    public string VerificationLabel => IsVerified ? "Terverifikasi" : "Belum terverifikasi";
}