using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace GigRadarApi.Data;

/// <summary>
/// EnsureCreated() hanya membuat skema saat database belum ada — tabel/kolom baru
/// tidak otomatis muncul di database SQLite yang sudah ada. Helper ini menambahkan
/// kolom & tabel baru (tipe tiket, data pembeli, link pembelian eksternal) secara
/// idempoten tanpa menghapus data yang ada.
/// </summary>
public static class TicketSchemaBootstrap
{
    public static void Ensure(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        try
        {
            var eventsCols = GetColumns(conn, "Events");
            if (!eventsCols.Contains("TicketLink"))
                Execute(conn, "ALTER TABLE Events ADD COLUMN TicketLink TEXT NOT NULL DEFAULT '';");

            var ticketsCols = GetColumns(conn, "Tickets");
            if (!ticketsCols.Contains("BuyerName"))
                Execute(conn, "ALTER TABLE Tickets ADD COLUMN BuyerName TEXT NOT NULL DEFAULT '';");
            if (!ticketsCols.Contains("BuyerPhone"))
                Execute(conn, "ALTER TABLE Tickets ADD COLUMN BuyerPhone TEXT NOT NULL DEFAULT '';");
            if (!ticketsCols.Contains("BuyerEmail"))
                Execute(conn, "ALTER TABLE Tickets ADD COLUMN BuyerEmail TEXT NOT NULL DEFAULT '';");
            if (!ticketsCols.Contains("BuyerDateOfBirth"))
                Execute(conn, "ALTER TABLE Tickets ADD COLUMN BuyerDateOfBirth TEXT NULL;");

            // Tabel tipe tiket (kolom mengikuti konvensi EF Core untuk SQLite:
            // decimal -> TEXT, bool/integer -> INTEGER).
            Execute(conn, """
                CREATE TABLE IF NOT EXISTS "EventTicketTypes" (
                    "EventTicketTypeId" INTEGER NOT NULL CONSTRAINT "PK_EventTicketTypes" PRIMARY KEY AUTOINCREMENT,
                    "EventId" INTEGER NOT NULL,
                    "Name" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Price" TEXT NOT NULL,
                    "Stock" INTEGER NOT NULL,
                    "SortOrder" INTEGER NOT NULL
                );
                """);

            SeedTicketTypesIfEmpty(conn);
            SetDemoTicketLink(conn);
            SeedEoUserIfMissing(conn);
            EnsureArtistSchema(conn);
            SeedArtistDemo(conn);
        }
        finally
        {
            conn.Close();
        }
    }

    private static void SeedTicketTypesIfEmpty(DbConnection conn)
    {
        using var countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM EventTicketTypes;";
        var count = Convert.ToInt64(countCmd.ExecuteScalar() ?? 0);
        if (count > 0) return;

        Execute(conn, """
            INSERT INTO "EventTicketTypes" ("EventId", "Name", "Description", "Price", "Stock", "SortOrder") VALUES
                (1, 'Festival', 'Akses penuh area festival (standing)', '35000', 150, 1),
                (1, 'Tribun', 'Area tribun duduk dengan pemandangan panggung', '25000', 50, 2),
                (1, 'Bundling Festival + Merch', 'Tiket festival + kaos eksklusif band', '60000', 30, 3),
                (2, 'Festival', 'Akses penuh festival 1 hari', '50000', 300, 1),
                (2, 'Tribun', 'Area tribun duduk', '30000', 100, 2),
                (2, 'Bundling 2 Tiket', '2 tiket festival dengan harga hemat', '90000', 40, 3),
                (3, 'Festival', 'Akses penuh indie night', '25000', 350, 1),
                (3, 'Tribun', 'Area tribun duduk', '20000', 150, 2),
                (3, 'Bundling Festival + Minuman', 'Tiket festival + 1 minuman gratis', '35000', 60, 3);
            """);
    }

    private static void SetDemoTicketLink(DbConnection conn)
    {
        // Contoh venue yang hanya menyediakan link pembelian eksternal (event 3).
        Execute(conn, """
            UPDATE Events SET TicketLink = 'https://www.loket.com/underground-indie-night'
            WHERE EventId = 3 AND (TicketLink IS NULL OR TicketLink = '');
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
            findCmd.CommandText = "SELECT UserId FROM Users WHERE Email = 'eo@gigradar.com';";
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
                    INSERT INTO Users (Name, Email, PasswordHash, Role, City, Latitude, Longitude, PhotoUrl, CreatedAt)
                    VALUES (@Name, @Email, @PasswordHash, @Role, @City, 0, 0, '', @CreatedAt);
                    SELECT last_insert_rowid();
                    """;
                var p = insertCmd.Parameters;
                p.Add(CreateParameter(conn, "@Name", "Event Organizer"));
                p.Add(CreateParameter(conn, "@Email", "eo@gigradar.com"));
                p.Add(CreateParameter(conn, "@PasswordHash", BCrypt.Net.BCrypt.HashPassword("eo123")));
                p.Add(CreateParameter(conn, "@Role", "EO"));
                p.Add(CreateParameter(conn, "@City", "Jakarta"));
                p.Add(CreateParameter(conn, "@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
                eoId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

        // Event demo milik EO (hanya bila masih dipegang Admin seed, agar tidak menimpa kepemilikan yang sudah diubah).
        Execute(conn, $"UPDATE Events SET CreatedBy = {eoId} WHERE EventId = 1 AND CreatedBy = 1;");
    }

    /// <summary>
    /// Phase 3 (Artist): tambahkan kolom & tabel baru untuk database yang sudah ada
    /// (kolom mengikuti konvensi EF Core untuk SQLite).
    /// </summary>
    private static void EnsureArtistSchema(DbConnection conn)
    {
        var artistCols = GetColumns(conn, "Artists");
        if (!artistCols.Contains("UserId"))
            Execute(conn, "ALTER TABLE Artists ADD COLUMN UserId INTEGER NULL;");
        if (!artistCols.Contains("City"))
            Execute(conn, "ALTER TABLE Artists ADD COLUMN City TEXT NOT NULL DEFAULT '';");
        if (!artistCols.Contains("CoverUrl"))
            Execute(conn, "ALTER TABLE Artists ADD COLUMN CoverUrl TEXT NOT NULL DEFAULT '';");

        var trackCols = GetColumns(conn, "AudioTracks");
        if (!trackCols.Contains("CoverUrl"))
            Execute(conn, "ALTER TABLE AudioTracks ADD COLUMN CoverUrl TEXT NOT NULL DEFAULT '';");
        if (!trackCols.Contains("Genre"))
            Execute(conn, "ALTER TABLE AudioTracks ADD COLUMN Genre TEXT NOT NULL DEFAULT '';");
        if (!trackCols.Contains("ReleaseDate"))
            Execute(conn, "ALTER TABLE AudioTracks ADD COLUMN ReleaseDate TEXT NULL;");
        if (!trackCols.Contains("CreatedAt"))
            Execute(conn, "ALTER TABLE AudioTracks ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '2026-01-01 00:00:00';");

        Execute(conn, """
            CREATE TABLE IF NOT EXISTS "ArtistAlbums" (
                "AlbumId" INTEGER NOT NULL CONSTRAINT "PK_ArtistAlbums" PRIMARY KEY AUTOINCREMENT,
                "ArtistId" INTEGER NOT NULL,
                "Title" TEXT NOT NULL,
                "CoverUrl" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "ReleaseDate" TEXT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """);
        Execute(conn, """
            CREATE TABLE IF NOT EXISTS "ArtistPosts" (
                "PostId" INTEGER NOT NULL CONSTRAINT "PK_ArtistPosts" PRIMARY KEY AUTOINCREMENT,
                "ArtistId" INTEGER NOT NULL,
                "Title" TEXT NOT NULL,
                "Content" TEXT NOT NULL,
                "ImageUrl" TEXT NOT NULL,
                "IsPublished" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """);
        Execute(conn, """
            CREATE TABLE IF NOT EXISTS "ArtistMembers" (
                "MemberId" INTEGER NOT NULL CONSTRAINT "PK_ArtistMembers" PRIMARY KEY AUTOINCREMENT,
                "ArtistId" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Role" TEXT NOT NULL,
                "PhotoUrl" TEXT NOT NULL,
                "JoinedAt" TEXT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """);
        Execute(conn, """
            CREATE TABLE IF NOT EXISTS "ArtistJourneyItems" (
                "JourneyId" INTEGER NOT NULL CONSTRAINT "PK_ArtistJourneyItems" PRIMARY KEY AUTOINCREMENT,
                "ArtistId" INTEGER NOT NULL,
                "Title" TEXT NOT NULL,
                "Description" TEXT NOT NULL,
                "ImageUrl" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "Date" TEXT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            """);

        // Index unik: satu user hanya punya satu profil artist
        Execute(conn, "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Artists_UserId\" ON \"Artists\" (\"UserId\");");
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
            findCmd.CommandText = "SELECT UserId FROM Users WHERE Email = 'artist@gigradar.com';";
            var result = findCmd.ExecuteScalar();
            if (result != null)
            {
                artistUserId = Convert.ToInt32(result);
            }
            else
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = """
                    INSERT INTO Users (Name, Email, PasswordHash, Role, City, Latitude, Longitude, PhotoUrl, CreatedAt)
                    VALUES (@Name, @Email, @PasswordHash, @Role, @City, 0, 0, '', @CreatedAt);
                    SELECT last_insert_rowid();
                    """;
                var p = insertCmd.Parameters;
                p.Add(CreateParameter(conn, "@Name", "Hollow Men"));
                p.Add(CreateParameter(conn, "@Email", "artist@gigradar.com"));
                p.Add(CreateParameter(conn, "@PasswordHash", BCrypt.Net.BCrypt.HashPassword("artist123")));
                p.Add(CreateParameter(conn, "@Role", "Artist"));
                p.Add(CreateParameter(conn, "@City", "Jakarta"));
                p.Add(CreateParameter(conn, "@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
                artistUserId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

        // 2) Hubungkan profil artist (Hollow Men) ke akun tersebut (hanya bila belum terhubung)
        Execute(conn, $"UPDATE Artists SET UserId = {artistUserId} WHERE ArtistId = 1 AND (UserId IS NULL OR UserId = 0);");

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
                    (1, 'Single baru: Afterglow', 'Single terbaru kami ''Afterglow'' sudah bisa didengarkan. Terima kasih yang sudah menunggu! 🌙', '', 1, '2026-01-20 00:00:00', '2026-01-20 00:00:00'),
                    (1, 'Tampil di Night of Shoegaze', 'Kami akan tampil di Night of Shoegaze bulan September. Siapkan telingamu! 🎸', '', 1, '2026-08-01 00:00:00', '2026-08-01 00:00:00');
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
        Execute(conn, "INSERT OR IGNORE INTO Follows (UserId, ArtistId, CreatedAt) VALUES (1, 1, '2026-01-01 00:00:00');");
        Execute(conn, "INSERT OR IGNORE INTO Follows (UserId, ArtistId, CreatedAt) VALUES (2, 1, '2026-01-01 00:00:00');");
    }

    private static bool TableIsEmpty(DbConnection conn, string table)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM \"{table}\";";
        return Convert.ToInt64(cmd.ExecuteScalar() ?? 0) == 0;
    }

    private static DbParameter CreateParameter(DbConnection conn, string name, object value)
    {
        var param = conn.CreateCommand().CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    private static List<string> GetColumns(DbConnection conn, string table)
    {
        var result = new List<string>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{table}\");";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(reader.GetString(1));
        return result;
    }

    private static void Execute(DbConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}