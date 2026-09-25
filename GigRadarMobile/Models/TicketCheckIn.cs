namespace GigRadarMobile.Models
{
    /// <summary>
    /// Hasil check-in QR dari server (POST /api/tickets/validate).
    /// CheckedIn/Total dibuat nullable: respons penolakan (mis. 403 bukan event milik EO
    /// ini) tidak menyertakan angka, dan UI tidak boleh menganggapnya 0.
    /// </summary>
    public class TicketCheckInResult
    {
        public bool Valid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CheckedIn { get; set; }
        public int? Total { get; set; }
        public CheckedInTicket? Ticket { get; set; }
    }

    /// <summary>Ringkas data tiket yang berhasil di-check-in (untuk ditampilkan ke petugas).</summary>
    public class CheckedInTicket
    {
        public int TicketId { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
