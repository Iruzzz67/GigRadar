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
        /// Registrasi sesuai GIGRADAR_APP_SPEC.md (Bab 7/13) — permohonan role Artist/EO
        /// Client BOLEH memohon role Artist/EO saat daftar, tetapi permohonan hanya
        /// disimpan sebagai RoleStatus "Pending" — role tetap "User" sampai diverifikasi
        /// admin. Role "Admin" tidak pernah bisa diminta dari client.
        /// </summary>
        public async Task<(bool Success, string Message, User? User, string? Token)> RegisterAsync(string name, string email, string password, string? requestedRole = null)
        {
            email = email.Trim();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
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

                if (normalizedRequest == "Artist")
                {
                    var artist = new Artist
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        VerificationStatus = "PENDING",
                        IsVerified = false
                    };
                    _context.Artists.Add(artist);
                    await _context.SaveChangesAsync();

                    _context.ArtistVerifications.Add(new ArtistVerification
                    {
                        ArtistId = artist.ArtistId,
                        LegalName = user.Name,
                        SupportingInfo = "Pengajuan akun Artist dari proses registrasi.",
                        Status = "PENDING",
                        SubmittedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    var organizer = new Organizer
                    {
                        UserId = user.UserId,
                        OrganizationName = user.Name,
                        Location = user.City,
                        Description = "Pengajuan akun Event Organizer dari proses registrasi.",
                        VerificationStatus = "PENDING",
                        IsVerified = false
                    };
                    _context.Organizers.Add(organizer);
                    await _context.SaveChangesAsync();

                    _context.OrganizerVerifications.Add(new OrganizerVerification
                    {
                        OrganizerId = organizer.OrganizerId,
                        OrganizationName = organizer.OrganizationName,
                        Description = organizer.Description,
                        Location = organizer.Location,
                        Status = "PENDING",
                        SubmittedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
            }

            // Akun yang meminta Artist/EO belum boleh membuat sesi sebelum disetujui Admin.
            var token = normalizedRequest == null ? GenerateJwtToken(user) : null;
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

        // ── Admin: CRUD akun (§4.4, §19, §23) ─────────────

        /// <summary>Role yang boleh diatur Admin — sama dengan Role system (§4).</summary>
        public static readonly string[] ManageableRoles = { "User", "Artist", "EO", "Admin" };

        /// <summary>Status akun yang dikenal sistem (RoleStatus).</summary>
        public const string ActiveStatus = "None";
        public const string SuspendedStatus = "Suspended";

        /// <summary>
        /// Admin membuat akun baru untuk role apa pun (termasuk Artist/EO langsung aktif,
        /// tanpa lewat antrian verifikasi). Akun Artist/EO yang dibuat di sini sengaja
        /// memiliki profil kosong + RoleStatus aktif agar bisa langsung login.
        /// </summary>
        public async Task<(bool Success, string Message, User? User)> CreateAccountAsync(
            string name, string email, string password, string role, string city)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3)
                return (false, "Nama wajib diisi (minimal 3 karakter)", null);

            email = (email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                return (false, "Format email tidak valid", null);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Password wajib diisi (minimal 6 karakter)", null);

            var normalizedRole = NormalizeRole(role);
            if (normalizedRole == null)
                return (false, $"Role tidak valid. Gunakan: {string.Join(", ", ManageableRoles)}", null);

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                return (false, "Email sudah terdaftar", null);

            var user = new User
            {
                Name = name.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = normalizedRole,
                RoleStatus = ActiveStatus,
                City = (city ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Artist/EO yang dibuat Admin langsung punya profil agar shell-nya tidak kosong.
            await EnsureProfileForRoleAsync(user);

            return (true, $"Akun {normalizedRole} dibuat", user);
        }

        /// <summary>
        /// Admin memperbarui data akun. Password hanya berubah bila diisi; nilai null/""
        /// berarti "pertahankan yang lama" supaya form yang tidak menyertakan password
        /// tidak menghapus kredensial user.
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateAccountAsync(
            int userId, string? name, string? email, string? password, string? role, string? city)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Akun tidak ditemukan");

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Trim().Length < 3)
                    return (false, "Nama wajib diisi (minimal 3 karakter)");
                user.Name = name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.Trim();
                if (!IsValidEmail(normalizedEmail))
                    return (false, "Format email tidak valid");

                if (await _context.Users.AnyAsync(u => u.UserId != userId && u.Email.ToLower() == normalizedEmail.ToLower()))
                    return (false, "Email sudah dipakai akun lain");

                user.Email = normalizedEmail;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6)
                    return (false, "Password minimal 6 karakter");
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = NormalizeRole(role);
                if (normalizedRole == null)
                    return (false, $"Role tidak valid. Gunakan: {string.Join(", ", ManageableRoles)}");
                user.Role = normalizedRole;
            }

            if (city != null)
                user.City = city.Trim();

            await _context.SaveChangesAsync();
            await EnsureProfileForRoleAsync(user);

            return (true, "Akun diperbarui");
        }

        /// <summary>
        /// Admin menonaktifkan akun. Status "Suspended" ditolak saat login
        /// (lihat <see cref="LoginAsync"/>) — akun tetap ada beserta datanya,
        /// sehingga bisa dipulihkan, berbeda dengan delete.
        /// </summary>
        public async Task<(bool Success, string Message)> SetAccountSuspendedAsync(
            int userId, bool suspended, int actingAdminId)
        {
            if (userId == actingAdminId)
                return (false, "Tidak bisa menonaktifkan akun sendiri");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Akun tidak ditemukan");

            user.RoleStatus = suspended ? SuspendedStatus : ActiveStatus;
            await _context.SaveChangesAsync();

            return (true, suspended ? $"Akun {user.Name} dinonaktifkan" : $"Akun {user.Name} diaktifkan kembali");
        }

        /// <summary>
        /// Admin menghapus akun beserta data yang bergantung padanya. Operasi destruktif
        /// (§23) — karena itu ada dua pengaman: tidak boleh menghapus diri sendiri, dan
        /// Admin terakhir tidak boleh dihapus agar platform tidak terkunci.
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId, int actingAdminId)
        {
            if (userId == actingAdminId)
                return (false, "Tidak bisa menghapus akun sendiri");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Akun tidak ditemukan");

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return (false, "Admin terakhir tidak bisa dihapus");
            }

            // Relasi yang bergantung pada akun ini dibersihkan eksplisit supaya tidak ada
            // baris menggantung di profil artist/organizer miliknya.
            var follows = await _context.Follows.Where(f => f.UserId == userId).ToListAsync();
            var favorites = await _context.Favorites.Where(f => f.UserId == userId).ToListAsync();
            var preferences = await _context.UserPreferences.Where(p => p.UserId == userId).ToListAsync();

            _context.Follows.RemoveRange(follows);
            _context.Favorites.RemoveRange(favorites);
            _context.UserPreferences.RemoveRange(preferences);

            // Profil artist milik akun ini dilepas tautannya (profil & karyanya tetap ada
            // sebagai entitas publik) alih-alih ikut terhapus.
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
            if (artist != null) artist.UserId = null;

            var organizer = await _context.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organizer != null) organizer.UserId = null;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return (true, $"Akun {user.Name} dihapus");
        }

        private static string? NormalizeRole(string? role) => role?.Trim() switch
        {
            "User" or "user" or "USER" => "User",
            "Artist" or "artist" or "ARTIST" => "Artist",
            "EO" or "eo" => "EO",
            "Admin" or "admin" or "ADMIN" => "Admin",
            _ => null
        };

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

        /// <summary>Profil Artist/EO dibuat sekali bila belum ada (idempoten).</summary>
        private async Task EnsureProfileForRoleAsync(User user)
        {
            if (user.Role == "Artist" && !await _context.Artists.AnyAsync(a => a.UserId == user.UserId))
            {
                _context.Artists.Add(new Artist { UserId = user.UserId, Name = user.Name, City = user.City });
                await _context.SaveChangesAsync();
            }
            else if (user.Role == "EO" && !await _context.Organizers.AnyAsync(o => o.UserId == user.UserId))
            {
                _context.Organizers.Add(new Organizer
                {
                    UserId = user.UserId,
                    OrganizationName = user.Name,
                    Location = user.City
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string email, string password)
        {
            email = email.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, "Email atau password salah", null, null);

            if (user.RoleStatus == "Pending")
                return (false, "Akun kamu masih menunggu verifikasi Admin.", null, null);
            if (user.RoleStatus == "Rejected")
                return (false, "Pengajuan akun kamu ditolak Admin. Silakan hubungi Admin.", null, null);
            if (user.RoleStatus == SuspendedStatus)
                return (false, "Akun kamu sedang dinonaktifkan Admin.", null, null);

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
