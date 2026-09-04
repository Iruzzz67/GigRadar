using Microsoft.EntityFrameworkCore;
using GigRadarApi.Models;

namespace GigRadarApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<ArtistAlbum> ArtistAlbums { get; set; }
        public DbSet<ArtistPost> ArtistPosts { get; set; }
        public DbSet<ArtistMember> ArtistMembers { get; set; }
        public DbSet<ArtistJourneyItem> ArtistJourneyItems { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventArtist> EventArtists { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<EventTicketType> EventTicketTypes { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<AudioTrack> AudioTracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Satu user ber-role Artist maksimal satu profil artist
            modelBuilder.Entity<Artist>().HasIndex(a => a.UserId).IsUnique();

            // EventArtist composite key
            modelBuilder.Entity<EventArtist>()
                .HasIndex(ea => new { ea.EventId, ea.ArtistId })
                .IsUnique();

            // Follow unique constraint
            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.UserId, f.ArtistId })
                .IsUnique();

            // Favorite unique constraint
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.EventId })
                .IsUnique();

            // Seed Admin, EO & Artist User
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Name = "Admin", Email = "admin@gigradar.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin", City = "Jakarta" },
                new User { UserId = 2, Name = "Event Organizer", Email = "eo@gigradar.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("eo123"), Role = "EO", City = "Jakarta" },
                new User { UserId = 3, Name = "Hollow Men", Email = "artist@gigradar.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("artist123"), Role = "Artist", City = "Jakarta" }
            );

            // Seed Genres
            modelBuilder.Entity<Genre>().HasData(
                new Genre { GenreId = 1, Name = "Indie", Icon = "🎸" },
                new Genre { GenreId = 2, Name = "Alternative", Icon = "🎵" },
                new Genre { GenreId = 3, Name = "Rock", Icon = "🤘" },
                new Genre { GenreId = 4, Name = "Punk", Icon = "💀" },
                new Genre { GenreId = 5, Name = "Hardcore", Icon = "🔥" },
                new Genre { GenreId = 6, Name = "Shoegaze", Icon = "🌙" },
                new Genre { GenreId = 7, Name = "Emo", Icon = "💔" },
                new Genre { GenreId = 8, Name = "Metal", Icon = "⚔️" },
                new Genre { GenreId = 9, Name = "Jazz", Icon = "🎷" },
                new Genre { GenreId = 10, Name = "Folk", Icon = "🪕" },
                new Genre { GenreId = 11, Name = "Electronic", Icon = "🎛️" },
                new Genre { GenreId = 12, Name = "Pop", Icon = "🎤" }
            );

            // Seed Venues
            modelBuilder.Entity<Venue>().HasData(
                new Venue { VenueId = 1, Name = "Graha Bhakti Budaya", Address = "Jl. Taman Ismail Marzuki", City = "Jakarta", Latitude = -6.1944, Longitude = 106.8329, Capacity = 1200 },
                new Venue { VenueId = 2, Name = "Gedung Kesenian", Address = "Jl. Sinabun", City = "Bandung", Latitude = -6.8849, Longitude = 107.6143, Capacity = 500 },
                new Venue { VenueId = 3, Name = "Bentara Budaya", Address = "Jl. Palmerah Selatan", City = "Jakarta", Latitude = -6.2297, Longitude = 106.8184, Capacity = 300 }
            );

            // Seed Artists (Artist 1 = profil yang dikelola akun artist@gigradar.com)
            modelBuilder.Entity<Artist>().HasData(
                new Artist { ArtistId = 1, UserId = 3, Name = "Hollow Men", Bio = "Shoegaze band dari Jakarta. Suara bising yang hangat, melodi yang melayang.", Genre = "Shoegaze", City = "Jakarta", CoverUrl = "" },
                new Artist { ArtistId = 2, Name = "Concrete Beach", Bio = "Midwest emo vibes", Genre = "Midwest Emo", PhotoUrl = "" },
                new Artist { ArtistId = 3, Name = "Static Bloom", Bio = "Indie rock with shoegaze influences", Genre = "Indie Rock", PhotoUrl = "" },
                new Artist { ArtistId = 4, Name = "Pale Circles", Bio = "Post-punk from the underground", Genre = "Post-Punk", PhotoUrl = "" },
                new Artist { ArtistId = 5, Name = "Soft Collapse", Bio = "Dream pop from Bogor", Genre = "Dream Pop", PhotoUrl = "" },
                new Artist { ArtistId = 6, Name = "Meridian", Bio = "Math rock with complex time signatures", Genre = "Math Rock", PhotoUrl = "" }
            );

            // Seed konten demo untuk Hollow Men (Artist 1)
            modelBuilder.Entity<AudioTrack>().HasData(
                new AudioTrack { TrackId = 1, ArtistId = 1, Title = "Static Haze", AudioUrl = "", CoverUrl = "", Genre = "Shoegaze", DurationSeconds = 210, ReleaseDate = new DateTime(2025, 3, 14) },
                new AudioTrack { TrackId = 2, ArtistId = 1, Title = "Midnight Bloom", AudioUrl = "", CoverUrl = "", Genre = "Shoegaze", DurationSeconds = 245, ReleaseDate = new DateTime(2025, 8, 2) },
                new AudioTrack { TrackId = 3, ArtistId = 1, Title = "Afterglow", AudioUrl = "", CoverUrl = "", Genre = "Dream Pop", DurationSeconds = 198, ReleaseDate = new DateTime(2026, 1, 20) }
            );

            modelBuilder.Entity<ArtistAlbum>().HasData(
                new ArtistAlbum { AlbumId = 1, ArtistId = 1, Title = "Pale Light", Description = "Album pertama — 10 lagu shoegaze.", ReleaseDate = new DateTime(2026, 2, 10) }
            );

            modelBuilder.Entity<ArtistPost>().HasData(
                new ArtistPost { PostId = 1, ArtistId = 1, Title = "Single baru: Afterglow", Content = "Single terbaru kami 'Afterglow' sudah bisa didengarkan. Terima kasih yang sudah menunggu! 🌙", IsPublished = true, CreatedAt = new DateTime(2026, 1, 20), UpdatedAt = new DateTime(2026, 1, 20) },
                new ArtistPost { PostId = 2, ArtistId = 1, Title = "Tampil di Night of Shoegaze", Content = "Kami akan tampil di Night of Shoegaze bulan September. Siapkan telingamu! 🎸", IsPublished = true, CreatedAt = new DateTime(2026, 8, 1), UpdatedAt = new DateTime(2026, 8, 1) }
            );

            modelBuilder.Entity<ArtistMember>().HasData(
                new ArtistMember { MemberId = 1, ArtistId = 1, Name = "Raka", Role = "Vokal & Gitar", JoinedAt = new DateTime(2024, 1, 15) },
                new ArtistMember { MemberId = 2, ArtistId = 1, Name = "Bima", Role = "Gitar", JoinedAt = new DateTime(2024, 1, 15) },
                new ArtistMember { MemberId = 3, ArtistId = 1, Name = "Sari", Role = "Bass", JoinedAt = new DateTime(2024, 3, 1) },
                new ArtistMember { MemberId = 4, ArtistId = 1, Name = "Dimas", Role = "Drum", JoinedAt = new DateTime(2024, 5, 20) }
            );

            modelBuilder.Entity<ArtistJourneyItem>().HasData(
                new ArtistJourneyItem { JourneyId = 1, ArtistId = 1, Title = "Band terbentuk", Description = "Hollow Men terbentuk di garasi Raka, Jakarta Selatan.", Category = "Formation", Date = new DateTime(2024, 1, 15) },
                new ArtistJourneyItem { JourneyId = 2, ArtistId = 1, Title = "Single pertama", Description = "'Static Haze' dirilis di platform digital.", Category = "Single", Date = new DateTime(2025, 3, 14) },
                new ArtistJourneyItem { JourneyId = 3, ArtistId = 1, Title = "Showcase pertama", Description = "Tampil di showcase underground pertama di Bentara Budaya.", Category = "Gig", Date = new DateTime(2025, 9, 5) },
                new ArtistJourneyItem { JourneyId = 4, ArtistId = 1, Title = "Album pertama: Pale Light", Description = "Album perdana dirilis dengan 10 lagu.", Category = "Album", Date = new DateTime(2026, 2, 10) }
            );

            // Seed follower: Admin & EO mengikuti Hollow Men
            modelBuilder.Entity<Follow>().HasData(
                new Follow { FollowId = 1, UserId = 1, ArtistId = 1 },
                new Follow { FollowId = 2, UserId = 2, ArtistId = 1 }
            );

            // Seed Events
            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, Name = "Night of Shoegaze", Description = "Night penuh dengan suara Shoegaze dan Post-Punk", PosterUrl = "", VenueId = 3, StartDate = new DateTime(2026, 9, 5, 20, 0, 0), EndDate = new DateTime(2026, 9, 5, 23, 0, 0), Latitude = -6.2297, Longitude = 106.8184, GenreId = 6, CreatedBy = 2, MinPrice = 25000, MaxPrice = 35000, Capacity = 200 },
                new Event { EventId = 2, Name = "Midwest Emo Fest", Description = "Festival emo terbesar di Bandung", PosterUrl = "", VenueId = 2, StartDate = new DateTime(2026, 9, 12, 19, 0, 0), EndDate = new DateTime(2026, 9, 12, 23, 0, 0), Latitude = -6.8849, Longitude = 107.6143, GenreId = 7, CreatedBy = 1, MinPrice = 30000, MaxPrice = 50000, Capacity = 400 },
                new Event { EventId = 3, Name = "Underground Indie Night", Description = "Indie night di tengah kota Jakarta", PosterUrl = "", VenueId = 1, StartDate = new DateTime(2026, 9, 19, 21, 0, 0), EndDate = new DateTime(2026, 9, 20, 0, 0, 0), Latitude = -6.1944, Longitude = 106.8329, GenreId = 1, CreatedBy = 1, MinPrice = 20000, MaxPrice = 25000, Capacity = 500 }
            );

            // Seed Ticket Types (Festival / Tribun / Bundling)
            modelBuilder.Entity<EventTicketType>().HasData(
                // Night of Shoegaze
                new EventTicketType { EventTicketTypeId = 1, EventId = 1, Name = "Festival", Description = "Akses penuh area festival (standing)", Price = 35000, Stock = 150, SortOrder = 1 },
                new EventTicketType { EventTicketTypeId = 2, EventId = 1, Name = "Tribun", Description = "Area tribun duduk dengan pemandangan panggung", Price = 25000, Stock = 50, SortOrder = 2 },
                new EventTicketType { EventTicketTypeId = 3, EventId = 1, Name = "Bundling Festival + Merch", Description = "Tiket festival + kaos eksklusif band", Price = 60000, Stock = 30, SortOrder = 3 },
                // Midwest Emo Fest
                new EventTicketType { EventTicketTypeId = 4, EventId = 2, Name = "Festival", Description = "Akses penuh festival 1 hari", Price = 50000, Stock = 300, SortOrder = 1 },
                new EventTicketType { EventTicketTypeId = 5, EventId = 2, Name = "Tribun", Description = "Area tribun duduk", Price = 30000, Stock = 100, SortOrder = 2 },
                new EventTicketType { EventTicketTypeId = 6, EventId = 2, Name = "Bundling 2 Tiket", Description = "2 tiket festival dengan harga hemat", Price = 90000, Stock = 40, SortOrder = 3 },
                // Underground Indie Night
                new EventTicketType { EventTicketTypeId = 7, EventId = 3, Name = "Festival", Description = "Akses penuh indie night", Price = 25000, Stock = 350, SortOrder = 1 },
                new EventTicketType { EventTicketTypeId = 8, EventId = 3, Name = "Tribun", Description = "Area tribun duduk", Price = 20000, Stock = 150, SortOrder = 2 },
                new EventTicketType { EventTicketTypeId = 9, EventId = 3, Name = "Bundling Festival + Minuman", Description = "Tiket festival + 1 minuman gratis", Price = 35000, Stock = 60, SortOrder = 3 }
            );

            // Seed EventArtists
            modelBuilder.Entity<EventArtist>().HasData(
                new EventArtist { Id = 1, EventId = 1, ArtistId = 1, Order = 1 },
                new EventArtist { Id = 2, EventId = 1, ArtistId = 4, Order = 2 },
                new EventArtist { Id = 3, EventId = 2, ArtistId = 2, Order = 1 },
                new EventArtist { Id = 4, EventId = 2, ArtistId = 3, Order = 2 },
                new EventArtist { Id = 5, EventId = 3, ArtistId = 5, Order = 1 },
                new EventArtist { Id = 6, EventId = 3, ArtistId = 6, Order = 2 }
            );
        }
    }
}
