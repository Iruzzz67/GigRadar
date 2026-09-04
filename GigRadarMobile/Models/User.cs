namespace GigRadarMobile.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string City { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<UserPreference> Preferences { get; set; } = new();

        /// <summary>Label role untuk tampilan (konsol Admin).</summary>
        public string RoleDisplay => Role switch
        {
            "Admin" => "Admin",
            "EO" => "Event Organizer",
            "Artist" => "Artist",
            _ => "User"
        };

        /// <summary>Warna badge role.</summary>
        public Color RoleColor => Role switch
        {
            "Admin" => Color.FromArgb("#39FF14"),
            "EO" => Color.FromArgb("#7B2FFF"),
            "Artist" => Color.FromArgb("#FFB020"),
            _ => Color.FromArgb("#B0B0B0")
        };
    }

    public class UserPreference
    {
        public int PreferenceId { get; set; }
        public int UserId { get; set; }
        public int GenreId { get; set; }
        public Genre? Genre { get; set; }
    }
}
