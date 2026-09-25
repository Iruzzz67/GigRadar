using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Services
{
    public class TicketService
    {
        private readonly AppDbContext _context;

        public TicketService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Umur minimal pembeli (verifikasi kelayakan membeli tiket).</summary>
        public const int MinimumAge = 17;

        /// <summary>
        /// Membeli tiket dengan alur verifikasi: tipe tiket valid -> data diri pembeli
        /// lengkap -> umur memenuhi syarat -> stok tersedia.
        /// </summary>
        public async Task<(Ticket? Ticket, string? Error)> PurchaseTicketAsync(
            int userId, int eventId, int ticketTypeId,
            string buyerName, string buyerPhone, string buyerEmail, DateTime? buyerDateOfBirth)
        {
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null)
                return (null, "Event tidak ditemukan");

            if (evt.Status != "Published")
                return (null, "Event ini tidak menerima pembelian tiket");

            var type = await _context.EventTicketTypes
                .FirstOrDefaultAsync(t => t.EventTicketTypeId == ticketTypeId && t.EventId == eventId);
            if (type == null)
                return (null, "Tipe tiket tidak ditemukan untuk event ini");

            if (type.Stock <= 0)
                return (null, "Tiket sudah habis (sold out)");

            // ── Verifikasi data diri pembeli ──────────────
            if (string.IsNullOrWhiteSpace(buyerName) || buyerName.Trim().Length < 3)
                return (null, "Nama lengkap wajib diisi (minimal 3 karakter)");

            if (string.IsNullOrWhiteSpace(buyerPhone) || !Regex.IsMatch(buyerPhone.Trim(), @"^[0-9+\-\s]{9,}$"))
                return (null, "Nomor telepon tidak valid (minimal 9 digit)");

            if (string.IsNullOrWhiteSpace(buyerEmail) || !IsValidEmail(buyerEmail.Trim()))
                return (null, "Format email tidak valid");

            if (buyerDateOfBirth == null)
                return (null, "Tanggal lahir wajib diisi");

            var age = CalculateAge(buyerDateOfBirth.Value);
            if (age < MinimumAge)
                return (null, $"Maaf, kamu belum memenuhi syarat umur minimal {MinimumAge} tahun untuk membeli tiket ini");

            // ── Simpan tiket & kurangi stok ───────────────
            // Stok dikurangi lewat UPDATE atomik ("Stock = Stock - 1 WHERE Stock > 0"),
            // bukan dengan menimpa nilai yang sudah dibaca di atas. Tanpa ini, dua
            // pembelian bersamaan pada stok terakhir bisa lolos berdua (oversell — §24).
            // Ticket + pengurangan stok dibungkus satu transaksi agar stok tidak
            // berkurang saat penyimpanan tiket justru gagal.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var stockTaken = await _context.EventTicketTypes
                .Where(t => t.EventTicketTypeId == ticketTypeId && t.EventId == eventId && t.Stock > 0)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Stock, t => t.Stock - 1));

            if (stockTaken == 0)
                return (null, "Tiket sudah habis (sold out)");

            // Harga & nama tipe selalu dari server — body request tidak boleh menentukan harga.
            var ticket = new Ticket
            {
                EventId = eventId,
                UserId = userId,
                TicketType = type.Name,
                Price = type.Price,
                BuyerName = buyerName.Trim(),
                BuyerPhone = buyerPhone.Trim(),
                BuyerEmail = buyerEmail.Trim(),
                BuyerDateOfBirth = buyerDateOfBirth.Value,
                QRCode = Guid.NewGuid().ToString("N").ToUpper(),
                Status = "Active",
                PurchasedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var saved = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e!.Venue)
                .FirstOrDefaultAsync(t => t.TicketId == ticket.TicketId);

            return (saved, null);
        }

        public async Task<List<EventTicketType>> GetEventTicketTypesAsync(int eventId)
        {
            return await _context.EventTicketTypes
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        // ── Kelola tipe tiket (halaman admin/EO) ──────────

        public async Task<(EventTicketType? Type, string? Error)> AddTicketTypeAsync(
            int eventId, EventTicketType type, int userId, string role)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null)
                return (null, "Event tidak ditemukan");

            if (!await CanManageEventAsync(eventId, userId, role))
                return (null, "Kamu tidak memiliki akses untuk mengelola event ini");

            var error = ValidateTicketType(type);
            if (error != null) return (null, error);

            type.EventTicketTypeId = 0;
            type.EventId = eventId;
            type.Name = type.Name.Trim();
            type.Description = type.Description?.Trim() ?? string.Empty;

            if (type.SortOrder <= 0)
            {
                var maxOrder = await _context.EventTicketTypes
                    .Where(t => t.EventId == eventId)
                    .Select(t => (int?)t.SortOrder)
                    .MaxAsync() ?? 0;
                type.SortOrder = maxOrder + 1;
            }

            _context.EventTicketTypes.Add(type);
            await _context.SaveChangesAsync();
            return (type, null);
        }

        public async Task<(EventTicketType? Type, string? Error)> UpdateTicketTypeAsync(
            int typeId, EventTicketType type, int userId, string role)
        {
            var entity = await _context.EventTicketTypes.FindAsync(typeId);
            if (entity == null)
                return (null, "Tipe tiket tidak ditemukan");

            if (!await CanManageEventAsync(entity.EventId, userId, role))
                return (null, "Kamu tidak memiliki akses untuk mengelola event ini");

            var error = ValidateTicketType(type);
            if (error != null) return (null, error);

            entity.Name = type.Name.Trim();
            entity.Description = type.Description?.Trim() ?? string.Empty;
            entity.Price = type.Price;
            entity.Stock = type.Stock;
            entity.SortOrder = type.SortOrder > 0 ? type.SortOrder : entity.SortOrder;

            await _context.SaveChangesAsync();
            return (entity, null);
        }

        public async Task<(bool Success, string? Error)> DeleteTicketTypeAsync(
            int typeId, int userId, string role)
        {
            var entity = await _context.EventTicketTypes.FindAsync(typeId);
            if (entity == null)
                return (false, "Tipe tiket tidak ditemukan");

            if (!await CanManageEventAsync(entity.EventId, userId, role))
                return (false, "Kamu tidak memiliki akses untuk mengelola event ini");

            _context.EventTicketTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        /// <summary>Admin boleh kelola semua event; EO hanya event yang ia buat.</summary>
        private async Task<bool> CanManageEventAsync(int eventId, int userId, string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            var evt = await _context.Events.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            return evt != null && evt.CreatedBy == userId;
        }

        private static string? ValidateTicketType(EventTicketType type)
        {
            if (type == null)
                return "Body tipe tiket wajib dikirim";

            if (string.IsNullOrWhiteSpace(type.Name))
                return "Nama tipe tiket wajib diisi";

            if (type.Price < 0)
                return "Harga tidak boleh negatif";

            if (type.Stock < 0)
                return "Stok tidak boleh negatif";

            return null;
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e!.Venue)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e!.Venue)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        /// <summary>
        /// Check-in tiket lewat QR (GIGRADAR_APP_SPEC.md Bab 14). Validasi dilakukan
        /// di server, bukan dengan mempercayai hasil scan client:
        ///   1. EO/Admin harus mengelola event yang sedang di-check-in (§24 — EO hanya
        ///      boleh mengurus event miliknya, jadi tiket EO lain tidak bisa "dibakar").
        ///   2. QR harus terdaftar pada event tersebut — token event lain ditolak tanpa
        ///      membocorkan detailnya.
        ///   3. Hanya tiket berstatus Active yang boleh berubah menjadi Used, sehingga
        ///      tiket yang sudah dipakai tidak bisa dipakai dua kali.
        /// </summary>
        public async Task<(Ticket? Ticket, string? Error, bool Forbidden)> CheckInTicketAsync(
            string qrCode, int eventId, int userId, string role)
        {
            var code = qrCode?.Trim() ?? string.Empty;
            if (code.Length == 0)
                return (null, "QR tiket wajib diisi", false);

            if (eventId <= 0)
                return (null, "Event check-in wajib dipilih", false);

            if (!await CanManageEventAsync(eventId, userId, role))
                return (null, "Kamu tidak memiliki akses untuk event ini", true);

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .ThenInclude(e => e!.Venue)
                .FirstOrDefaultAsync(t => t.QRCode == code && t.EventId == eventId);

            if (ticket == null)
                return (null, "Tiket tidak dikenali untuk event ini", false);

            if (ticket.Status == "Used")
                return (null, "Tiket ini sudah pernah dipakai", false);

            if (ticket.Status != "Active")
                return (null, $"Tiket tidak berstatus aktif (status: {ticket.Status})", false);

            ticket.Status = "Used";
            await _context.SaveChangesAsync();

            return (ticket, null, false);
        }

        /// <summary>
        /// Daftar peserta (seluruh tiket) sebuah event untuk EO pemiliknya / Admin (§14, §30).
        /// Kepemilikan event diperiksa lebih dulu (§24) — EO lain hanya dapat pesan 403.
        /// Entitas Ticket dikembalikan apa adanya; controller yang menyusun respons
        /// sehingga data kontak pembeli tidak ikut terkirim.
        /// </summary>
        public async Task<(List<Ticket>? Attendees, string? Error, bool Forbidden)> GetEventAttendeesAsync(
            int eventId, int userId, string role)
        {
            if (eventId <= 0)
                return (null, "Event tidak ditemukan", false);

            if (!await CanManageEventAsync(eventId, userId, role))
                return (null, "Kamu tidak memiliki akses untuk event ini", true);

            var attendees = await _context.Tickets
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();

            return (attendees, null, false);
        }

        /// <summary>Ringkasan kehadiran event untuk halaman check-in ("Checked In 127 / 500").</summary>
        public async Task<(int CheckedIn, int Total)> GetCheckInCountsAsync(int eventId)
        {
            var total = await _context.Tickets.CountAsync(t => t.EventId == eventId);
            var checkedIn = await _context.Tickets.CountAsync(t => t.EventId == eventId && t.Status == "Used");
            return (checkedIn, total);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}