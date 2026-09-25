using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Data;

/// <summary>
/// Dulu helper ini menambal skema database yang dibuat via EnsureCreated() — tabel/kolom
/// baru tidak otomatis muncul di database SQLite yang sudah ada, jadi ditambahkan manual
/// (PRAGMA table_info, ALTER TABLE, AUTOINCREMENT, last_insert_rowid).
/// Sekarang skema dimiliki EF migrations (Migrate() di Program.cs), sehingga patching
/// skema tidak diperlukan lagi. Yang tetap dipertahankan: seeding data demo secara
/// idempoten (akun EO/Artist, tipe tiket, konten artist, follower demo) untuk database
/// yang tabelnya masih kosong, plus nilai demo TicketLink.
/// Sintaks SQL sudah diadaptasi dari SQLite ke PostgreSQL:
///   last_insert_rowid()  -> RETURNING
///   INSERT OR IGNORE     -> ON CONFLICT DO NOTHING
///   '35000' (TEXT)       -> 35000 (numeric)
/// </summary>
public static class TicketSchemaBootstrap
{
    public static void Ensure(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        try
        {
            SeedTicketTypesIfEmpty(conn);
            SetDemoTicketLink(conn);
            SeedEoUserIfMissing(conn);
            SeedArtistDemo(conn);
            NormalizeVerificationStatus(conn);
            EnsureDemoOrganizerVerified(db);
        }
        finally
        {
            conn.Close();
        }
    }

    /// <summary>
    /// Kolom VerificationStatus ditambahkan migration dengan default "" (perilaku standar
    /// EF untuk kolom baru NOT NULL tanpa defaultValue eksplisit). Normalisasi idempoten:
    /// baris yang masih kosong dianggap NOT_SUBMITTED (Bab 8).
    /// </summary>
    private static void NormalizeVerificationStatus(DbConnection conn)
    {
        Execute(conn,
            """UPDATE "Artists" SET "VerificationStatus" = 'NOT_SUBMITTED' WHERE "VerificationStatus" = '';""");
    }

    private static void SeedTicketTypesIfEmpty(DbConnection conn)
    {
        if (!TableIsEmpty(conn, "EventTicketTypes")) return;

        Execute(conn, """
            INSERT INTO "EventTicketTypes" ("EventId", "Name", "Description", "Price", "Stock", "SortOrder") VALUES
                (1, 'Festival', 'Akses penuh area festival (standing)', 35000, 150, 1),
                (1, 'Tribun', 'Area tribun duduk dengan pemandangan panggung', 25000, 50, 2),
                (1, 'Bundling Festival + Merch', 'Tiket festival + kaos eksklusif band', 60000, 30, 3),
                (2, 'Festival', 'Akses penuh festival 1 hari', 50000, 300, 1),
                (2, 'Tribun', 'Area tribun duduk', 30000, 100, 2),
                (2, 'Bundling 2 Tiket', '2 tiket festival dengan harga hemat', 90000, 40, 3),
                (3, 'Festival', 'Akses penuh indie night', 25000, 350, 1),
                (3, 'Tribun', 'Area tribun duduk', 20000, 150, 2),
                (3, 'Bundling Festival + Minuman', 'Tiket festival + 1 minuman gratis', 35000, 60, 3);
            """);
    }

    private static void SetDemoTicketLink(DbConnection conn)
    {
        // Contoh venue yang hanya menyediakan link pembelian eksternal (event 3).
        Execute(conn, """
            UPDATE "Events" SET "TicketLink" = 'https://www.loket.com/underground-indie-night'
            WHERE "EventId" = 3 AND ("TicketLink" IS NULL OR "TicketLink" = '');
            """);
    }

    /// <summary>
    /// Pastikan akun demo EO (eo@gigradar.com / eo123) ada di database yang sudah
    /// terlanjur dibuat — seed HasData hanya berlaku untuk database baru.
    /// Event 1 (Night of Shoegaze) diserahkan ke EO agar halaman kelola tiket
    /// langsung berisi data saat dicoba.
    /// </summary>
    private static void SeedEoUserIfMissing(DbConnection conn)
    {
        var eoId = 0;
        using (var findCmd = conn.CreateCommand())
        {
            findCmd.CommandText = "SELECT \"UserId\" FROM \"Users\" WHERE \"Email\" = 'eo@gigradar.com';";
            var result = findCmd.ExecuteScalar();
            if (result != null)
            {
                eoId = Convert.ToInt32(result);
            }
            else
            {
                // Hash dibuat di sisi C# lalu disisipkan via parameter (BCrypt tidak bisa dipanggil di SQL).
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = """
                    INSERT INTO "Users" ("Name", "Email", "PasswordHash", "Role", "RoleStatus", "City", "Latitude", "Longitude", "PhotoUrl", "CreatedAt")
                    VALUES (@Name, @Email, @PasswordHash, @Role, 'None', @City, 0, 0, '', @CreatedAt)
                    RETURNING "UserId";
                    """;
                var p = insertCmd.Parameters;
                p.Add(CreateParameter("@Name", "Event Organizer"));
                p.Add(CreateParameter("@Email", "eo@gigradar.com"));
                p.Add(CreateParameter("@PasswordHash", BCrypt.Net.BCrypt.HashPassword("eo123")));
                p.Add(CreateParameter("@Role", "EO"));
                p.Add(CreateParameter("@City", "Jakarta"));
                p.Add(CreateParameter("@CreatedAt", DateTime.UtcNow));
                eoId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

        // Event demo milik EO (hanya bila masih dipegang Admin seed, agar tidak menimpa kepemilikan yang sudah diubah).
        Execute(conn, $"UPDATE \"Events\" SET \"CreatedBy\" = {eoId} WHERE \"EventId\" = 1 AND \"CreatedBy\" = 1;");
    }

    /// <summary>
    /// Gate verifikasi organizer (Bab 15) memblokir EO yang belum lolos review Admin
    /// dari membuat/mengubah event dan mengelola tiket. Supaya akun demo EO tetap bisa
    /// mencoba alur EO, profil organizer-nya dibuat sekali (idempoten) dan ditandai
    /// sudah diverifikasi Admin — database yang EO-nya sudah punya profil tidak disentuh.
    /// </summary>
    private static void EnsureDemoOrganizerVerified(AppDbContext db)
    {
        const string demoEoEmail = "eo@gigradar.com";
        var eoUserId = db.Users.Where(u => u.Email == demoEoEmail)
            .Select(u => (int?)u.UserId).FirstOrDefault();
        if (eoUserId == null) return;
        if (db.Organizers.Any(o => o.UserId == eoUserId)) return;

        var organizer = new Models.Organizer
        {
            UserId = eoUserId,
            OrganizationName = "GigRadar Demo Promotor",
            Description = "Akun demo EO — promotor gig lokal Jakarta.",
            Location = "Jakarta",
            Contact = "081234567890",
            FoundedYear = 2019,
            VerificationStatus = "VERIFIED",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Organizers.Add(organizer);
        db.SaveChanges();

        // Jejak audit keputusan Admin (Bab 15) agar EO demo konsisten dengan alur verifikasi.
        db.OrganizerVerifications.Add(new Models.OrganizerVerification
        {
            OrganizerId = organizer.OrganizerId,
            OrganizationName = organizer.OrganizationName,
            Description = organizer.Description,
            Location = organizer.Location,
            Contact = organizer.Contact,
            PortfolioSummary = "",
            Status = "VERIFIED",
            SubmittedAt = DateTime.UtcNow,
            ReviewedAt = DateTime.UtcNow,
            ReviewedByUserId = 1 // Admin seed
        });
        db.SaveChanges();
    }

    /// <summary>
    /// Akun demo Artist (artist@gigradar.com / artist123) + profil & konten demo
    /// untuk database lama — hanya diisi bila belum ada.
    /// </summary>
    private static void SeedArtistDemo(DbConnection conn)
    {
        // 1) Pastikan akun user Artist ada
        var artistUserId = 0;
        using (var findCmd = conn.CreateCommand())
        {
            findCmd.CommandText = "SELECT \"UserId\" FROM \"Users\" WHERE \"Email\" = 'artist@gigradar.com';";
            var result = findCmd.ExecuteScalar();
            if (result != null)
            {
                artistUserId = Convert.ToInt32(result);
            }
            else
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = """
                    INSERT INTO "Users" ("Name", "Email", "PasswordHash", "Role", "RoleStatus", "City", "Latitude", "Longitude", "PhotoUrl", "CreatedAt")
                    VALUES (@Name, @Email, @PasswordHash, @Role, 'None', @City, 0, 0, '', @CreatedAt)
                    RETURNING "UserId";
                    """;
                var p = insertCmd.Parameters;
                p.Add(CreateParameter("@Name", "Hollow Men"));
                p.Add(CreateParameter("@Email", "artist@gigradar.com"));
                p.Add(CreateParameter("@PasswordHash", BCrypt.Net.BCrypt.HashPassword("artist123")));
                p.Add(CreateParameter("@Role", "Artist"));
                p.Add(CreateParameter("@City", "Jakarta"));
                p.Add(CreateParameter("@CreatedAt", DateTime.UtcNow));
                artistUserId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

        // 2) Hubungkan profil artist (Hollow Men) ke akun tersebut (hanya bila belum terhubung)
        Execute(conn, $"UPDATE \"Artists\" SET \"UserId\" = {artistUserId} WHERE \"ArtistId\" = 1 AND (\"UserId\" IS NULL OR \"UserId\" = 0);");

        // 3) Konten demo hanya bila tabel masih kosong
        if (TableIsEmpty(conn, "AudioTracks"))
        {
            Execute(conn, """
                INSERT INTO "AudioTracks" ("ArtistId", "Title", "AudioUrl", "CoverUrl", "Genre", "DurationSeconds", "ReleaseDate", "CreatedAt") VALUES
                    (1, 'Static Haze', '', '', 'Shoegaze', 210, '2025-03-14 00:00:00', '2025-03-14 00:00:00'),
                    (1, 'Midnight Bloom', '', '', 'Shoegaze', 245, '2025-08-02 00:00:00', '2025-08-02 00:00:00'),
                    (1, 'Afterglow', '', '', 'Dream Pop', 198, '2026-01-20 00:00:00', '2026-01-20 00:00:00');
                """);
        }
        if (TableIsEmpty(conn, "ArtistAlbums"))
        {
            Execute(conn, """
                INSERT INTO "ArtistAlbums" ("ArtistId", "Title", "CoverUrl", "Description", "ReleaseDate", "CreatedAt") VALUES
                    (1, 'Pale Light', '', 'Album pertama — 10 lagu shoegaze.', '2026-02-10 00:00:00', '2026-02-10 00:00:00');
                """);
        }
        if (TableIsEmpty(conn, "ArtistPosts"))
        {
            Execute(conn, """
                INSERT INTO "ArtistPosts" ("ArtistId", "Title", "Content", "ImageUrl", "IsPublished", "CreatedAt", "UpdatedAt") VALUES
                    (1, 'Single baru: Afterglow', 'Single terbaru kami ''Afterglow'' sudah bisa didengarkan. Terima kasih yang sudah menunggu! 🌙', '', true, '2026-01-20 00:00:00', '2026-01-20 00:00:00'),
                    (1, 'Tampil di Night of Shoegaze', 'Kami akan tampil di Night of Shoegaze bulan September. Siapkan telingamu! 🎸', '', true, '2026-08-01 00:00:00', '2026-08-01 00:00:00');
                """);
        }
        if (TableIsEmpty(conn, "ArtistMembers"))
        {
            Execute(conn, """
                INSERT INTO "ArtistMembers" ("ArtistId", "Name", "Role", "PhotoUrl", "JoinedAt", "CreatedAt") VALUES
                    (1, 'Raka', 'Vokal & Gitar', '', '2024-01-15 00:00:00', '2024-01-15 00:00:00'),
                    (1, 'Bima', 'Gitar', '', '2024-01-15 00:00:00', '2024-01-15 00:00:00'),
                    (1, 'Sari', 'Bass', '', '2024-03-01 00:00:00', '2024-03-01 00:00:00'),
                    (1, 'Dimas', 'Drum', '', '2024-05-20 00:00:00', '2024-05-20 00:00:00');
                """);
        }
        if (TableIsEmpty(conn, "ArtistJourneyItems"))
        {
            Execute(conn, """
                INSERT INTO "ArtistJourneyItems" ("ArtistId", "Title", "Description", "ImageUrl", "Category", "Date", "CreatedAt") VALUES
                    (1, 'Band terbentuk', 'Hollow Men terbentuk di garasi Raka, Jakarta Selatan.', '', 'Formation', '2024-01-15 00:00:00', '2024-01-15 00:00:00'),
                    (1, 'Single pertama', '''Static Haze'' dirilis di platform digital.', '', 'Single', '2025-03-14 00:00:00', '2025-03-14 00:00:00'),
                    (1, 'Showcase pertama', 'Tampil di showcase underground pertama di Bentara Budaya.', '', 'Gig', '2025-09-05 00:00:00', '2025-09-05 00:00:00'),
                    (1, 'Album pertama: Pale Light', 'Album perdana dirilis dengan 10 lagu.', '', 'Album', '2026-02-10 00:00:00', '2026-02-10 00:00:00');
                """);
        }

        // 4) Follower demo (Admin & EO) — hanya bila belum ada
        Execute(conn, "INSERT INTO \"Follows\" (\"UserId\", \"ArtistId\", \"CreatedAt\") VALUES (1, 1, '2026-01-01 00:00:00') ON CONFLICT DO NOTHING;");
        Execute(conn, "INSERT INTO \"Follows\" (\"UserId\", \"ArtistId\", \"CreatedAt\") VALUES (2, 1, '2026-01-01 00:00:00') ON CONFLICT DO NOTHING;");
    }

    private static bool TableIsEmpty(DbConnection conn, string table)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM \"{table}\";";
        return Convert.ToInt64(cmd.ExecuteScalar() ?? 0) == 0;
    }

    private static DbParameter CreateParameter(string name, object value)
    {
        var param = new Npgsql.NpgsqlParameter(name, value);
        return param;
    }

    private static void Execute(DbConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
