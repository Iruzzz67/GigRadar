using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GigRadarApi.Data;
using GigRadarApi.Models;

namespace GigRadarApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Registrasi sesuai GIGRADAR_ROLE_SYSTEM.md §25/§51.2.
        /// Client BOLEH memohon role Artist/EO saat daftar, tetapi permohonan hanya
        /// disimpan sebagai RoleStatus "Pending" — role tetap "User" sampai diverifikasi
        /// admin. Role "Admin" tidak pernah bisa diminta dari client.
        /// </summary>
        public async Task<(bool Success, string Message, User? User, string? Token)> RegisterAsync(string name, string email, string password, string? requestedRole = null)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Email sudah terdaftar", null, null);

            // Normalisasi permohonan role: hanya Artist/EO yang bisa dimohonkan via register.
            var normalizedRequest = requestedRole?.Trim().ToUpperInvariant() switch
            {
                "ARTIST" => "Artist",
                "EO" => "EO",
                _ => null
            };

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User",
                RoleStatus = normalizedRequest == null ? "None" : "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (normalizedRequest != null)
            {
                _context.RoleRequests.Add(new RoleRequest
                {
                    UserId = user.UserId,
                    RequestedRole = normalizedRequest,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user);
            var message = normalizedRequest == null
                ? "Register berhasil"
                : $"Register berhasil. Permohonan akun {normalizedRequest} menunggu verifikasi admin.";
            return (true, message, user, token);
        }

        /// <summary>
        /// Admin menyetujui permohonan role: role user dinaikkan ke role yang dimohonkan
        /// dan untuk Artist dibuatkan profil Artist dasar (UserId terhubung, §6/§8).
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveRoleRequestAsync(int requestId, int adminId)
        {
            var request = await _context.RoleRequests.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null)
                return (false, "Permohonan tidak ditemukan");
            if (request.Status != "Pending")
                return (false, $"Permohonan sudah diproses ({request.Status})");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return (false, "User tidak ditemukan");

            user.Role = request.RequestedRole;
            user.RoleStatus = "Approved";
            request.Status = "Approved";
            request.ReviewedByAdminId = adminId;
            request.ReviewedAt = DateTime.UtcNow;

            // Auto-buat profil Artist dasar agar ArtistShell langsung punya profil.
            if (user.Role == "Artist" && !await _context.Artists.AnyAsync(a => a.UserId == user.UserId))
            {
                _context.Artists.Add(new Artist
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Genre = "",
                    City = user.City
                });
            }

            await _context.SaveChangesAsync();
            return (true, $"Permohonan disetujui — {user.Name} sekarang {user.Role}");
        }

        /// <summary>Admin menolak permohonan role: user tetap "User".</summary>
        public async Task<(bool Success, string Message)> RejectRoleRequestAsync(int requestId, int adminId)
        {
            var request = await _context.RoleRequests.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null)
                return (false, "Permohonan tidak ditemukan");
            if (request.Status != "Pending")
                return (false, $"Permohonan sudah diproses ({request.Status})");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user != null)
                user.RoleStatus = "None";
            request.Status = "Rejected";
            request.ReviewedByAdminId = adminId;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Permohonan ditolak — user tetap berrole User");
        }

        public async Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, "Email atau password salah", null, null);

            var token = GenerateJwtToken(user);
            return (true, "Login berhasil", user, token);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpirationMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
