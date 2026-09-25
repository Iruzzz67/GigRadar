namespace GigRadarMobile.Models
{
    /// <summary>
    /// Satu peserta (tiket) pada halaman Peserta EO
    /// (GET /api/tickets/event/{id}/attendees). Server tidak mengirim data kontak
    /// pembeli — hanya yang dibutuhkan saat check-in.
    /// </summary>
    public class EventAttendee
    {
        public int TicketId { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime PurchasedAt { get; set; }

        public string Initials => BuildInitials(BuyerName);
        public string PriceFormatted => $"Rp {Price:N0}";
        public string PurchasedLabel => PurchasedAt == default
            ? ""
            : $"Beli {PurchasedAt.ToLocalTime():dd MMM yyyy HH:mm}";

        public bool IsCheckedIn => Status == "Used";

        public string StatusLabel => Status switch
        {
            "Used" => "Sudah check-in",
            "Cancelled" => "Dibatalkan",
            _ => "Belum check-in"
        };

        public Color StatusColor => Status switch
        {
            "Used" => Color.FromArgb("#22C55E"),
            "Cancelled" => Color.FromArgb("#F43F5E"),
            _ => Color.FromArgb("#71717A")
        };

        private static string BuildInitials(string name)
        {
            var parts = (name ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }

    /// <summary>Ringkasan kehadiran + daftar peserta sebuah event.</summary>
    public class EventAttendeesReport
    {
        public int EventId { get; set; }
        public int Total { get; set; }
        public int CheckedIn { get; set; }
        public int Active { get; set; }
        public List<EventAttendee> Attendees { get; set; } = new();
    }
}
