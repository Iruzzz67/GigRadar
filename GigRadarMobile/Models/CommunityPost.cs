namespace GigRadarMobile.Models
{
    public class CommunityPost
    {
        public int CommunityPostId { get; set; }
        public int ArtistId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPhotoUrl { get; set; } = string.Empty;
    }
}
