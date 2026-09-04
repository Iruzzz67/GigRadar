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
            type.Stock -= 1;

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

        public async Task<bool> ValidateTicketAsync(string qrCode)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.QRCode == qrCode && t.Status == "Active");
            if (ticket == null) return false;

            ticket.Status = "Used";
            await _context.SaveChangesAsync();
            return true;
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