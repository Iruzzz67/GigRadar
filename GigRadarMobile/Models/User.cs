namespace GigRadarMobile.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        /// <summary>Status akun: None (aktif) / Pending / Approved / Suspended (§23).</summary>
        public string RoleStatus { get; set; } = "None";

        /// <summary>True bila akun dinonaktifkan Admin — hanya login yang ditolak.</summary>
        public bool IsSuspended => RoleStatus == "Suspended";

        /// <summary>Label tombol aksi nonaktifkan/aktifkan.</summary>
        public string SuspendActionLabel => IsSuspended ? "Aktifkan" : "Nonaktifkan";
        public string City { get; set; } = string.Empty;
        public string Initials => BuildInitials(Name);

        private static string BuildInitials(string name)
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
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

        /// <summary>Label status akun untuk tampilan.</summary>
        public string RoleStatusDisplay => RoleStatus switch
        {
            "Pending" => "⏳ Menunggu verifikasi",
            "Approved" => "✓ Terverifikasi",
            "Suspended" => "⛔ Dinonaktifkan",
            _ => string.Empty
        };

        /// <summary>Warna badge role (nilai literal = token design system).</summary>
        public Color RoleColor => Role switch
        {
            "Admin" => Color.FromArgb("#A3FF12"),   // PrimaryColor
            "EO" => Color.FromArgb("#7DD3FC"),      // GenreColors.Electronic
            "Artist" => Color.FromArgb("#F59E0B"),  // WarningColor
            _ => Color.FromArgb("#71717A")           // TextMuted
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
