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
            "Draft" => "Draft",
            _ => "Aktif"
        };
        public string SoldLabel => $"{TicketsSold} tiket terjual · {RemainingStock} stok tersisa";
        public Color StatusColor => Status switch
        {
            "SoldOut" => Color.FromArgb("#FF6B6B"),
            "Completed" => Color.FromArgb("#9E9E9E"),
            "Draft" => Color.FromArgb("#FFB020"),
            _ => Color.FromArgb("#39FF14")
        };

        /// <summary>Status aktif = Published (bisa dijual).</summary>
        public bool IsActive => Status == "Published";

        /// <summary>Tombol "Selesai" hanya masuk akal selama event aktif / sold out.</summary>
        public bool CanMarkCompleted => Status is "Published" or "SoldOut";

        /// <summary>Label aksi utama: tandai habis (saat aktif) atau aktifkan lagi (saat tidak aktif).</summary>
        public string PrimaryActionLabel => IsActive ? "🎟️ Tandai Habis" : "🔄 Aktifkan Lagi";
    }
}