namespace GigRadarMobile.Models
{
    /// <summary>Ringkasan dashboard profil EO dari GET /api/events/managed/summary.</summary>
    public class EoDashboard
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<EoEventStat> Events { get; set; } = new();

        public string TotalRevenueFormatted => $"Rp {TotalRevenue:N0}";
    }

    /// <summary>Statistik satu event milik EO/Admin untuk daftar ringkasan.</summary>
    public class EoEventStat
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Published";
        public DateTime StartDate { get; set; }
        public int Capacity { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public int TicketTypeCount { get; set; }
        public int RemainingStock { get; set; }

        public string DateLabel => StartDate.ToLocalTime().ToString("dd MMM yyyy");
        public string RevenueFormatted => $"Rp {Revenue:N0}";
        public string StatusLabel => Status switch
        {
            "SoldOut" => "Tiket Habis",
            "Completed" => "Selesai",
            "Draft" => "Draf",
            _ => "Aktif"
        };
        public string SoldLabel => $"{TicketsSold} tiket terjual · {RemainingStock} stok tersisa";
        // Nilai literal = token design system (SuccessColor/WarningColor/DangerColor/TextMuted)
        // karena C# tidak bisa memakai StaticResource.
        public Color StatusColor => Status switch
        {
            "SoldOut" => Color.FromArgb("#F43F5E"),
            "Completed" => Color.FromArgb("#71717A"),
            "Draft" => Color.FromArgb("#F59E0B"),
            _ => Color.FromArgb("#22C55E")
        };

        /// <summary>Status aktif = Published (bisa dijual).</summary>
        public bool IsActive => Status == "Published";

        /// <summary>Tombol "Selesai" hanya masuk akal selama event aktif / sold out.</summary>
        public bool CanMarkCompleted => Status is "Published" or "SoldOut";

        /// <summary>Label aksi utama: tandai habis (saat aktif) atau aktifkan lagi (saat tidak aktif).</summary>
        public string PrimaryActionLabel => IsActive ? "🎟️ Tandai Habis" : "🔄 Aktifkan Lagi";

        /// <summary>Label status bahasa tunggal (ID) — konsisten dengan badge halaman lain.</summary>
        public string StatusLabelId => StatusLabel;
    }
}