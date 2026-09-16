using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class PostgreSQLInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.GenreId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    RoleStatus = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                });

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    ArtistId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    CoverUrl = table.Column<string>(type: "text", nullable: false),
                    Genre = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    SocialLinks = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.ArtistId);
                    table.ForeignKey(
                        name: "FK_Artists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RoleRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RequestedRole = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_RoleRequests_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RoleRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    PreferenceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GenreId = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.PreferenceId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PosterUrl = table.Column<string>(type: "text", nullable: false),
                    TicketLink = table.Column<string>(type: "text", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    GenreId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    MinPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    ViewsCount = table.Column<int>(type: "integer", nullable: false),
                    SavesCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "GenreId");
                    table.ForeignKey(
                        name: "FK_Events_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId");
                });

            migrationBuilder.CreateTable(
                name: "ArtistAlbums",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CoverUrl = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAlbums", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_ArtistAlbums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistJourneyItems",
                columns: table => new
                {
                    JourneyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistJourneyItems", x => x.JourneyId);
                    table.ForeignKey(
                        name: "FK_ArtistJourneyItems_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMembers",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_ArtistMembers_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistPosts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistPosts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_ArtistPosts_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AudioTracks",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: false),
                    CoverUrl = table.Column<string>(type: "text", nullable: false),
                    Genre = table.Column<string>(type: "text", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioTracks", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_AudioTracks_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    FollowId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: true),
                    VenueId = table.Column<int>(type: "integer", nullable: true),
                    EventOrganizerId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.FollowId);
                    table.ForeignKey(
                        name: "FK_Follows_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId");
                    table.ForeignKey(
                        name: "FK_Follows_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Follows_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId");
                });

            migrationBuilder.CreateTable(
                name: "EventArtists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventArtists_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTicketTypes",
                columns: table => new
                {
                    EventTicketTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTicketTypes", x => x.EventTicketTypeId);
                    table.ForeignKey(
                        name: "FK_EventTicketTypes_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    FavoriteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.FavoriteId);
                    table.ForeignKey(
                        name: "FK_Favorites_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TicketType = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyerName = table.Column<string>(type: "text", nullable: false),
                    BuyerPhone = table.Column<string>(type: "text", nullable: false),
                    BuyerEmail = table.Column<string>(type: "text", nullable: false),
                    BuyerDateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    QRCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Tickets_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "ArtistId", "Bio", "City", "CoverUrl", "CreatedAt", "Genre", "Name", "PhotoUrl", "SocialLinks", "UserId" },
                values: new object[,]
                {
                    { 2, "Midwest emo vibes", "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7631), "Midwest Emo", "Concrete Beach", "", "", null },
                    { 3, "Indie rock with shoegaze influences", "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7637), "Indie Rock", "Static Bloom", "", "", null },
                    { 4, "Post-punk from the underground", "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7645), "Post-Punk", "Pale Circles", "", "", null },
                    { 5, "Dream pop from Bogor", "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7649), "Dream Pop", "Soft Collapse", "", "", null },
                    { 6, "Math rock with complex time signatures", "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7652), "Math Rock", "Meridian", "", "", null }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "GenreId", "Icon", "Name" },
                values: new object[,]
                {
                    { 1, "🎸", "Indie" },
                    { 2, "🎵", "Alternative" },
                    { 3, "🤘", "Rock" },
                    { 4, "💀", "Punk" },
                    { 5, "🔥", "Hardcore" },
                    { 6, "🌙", "Shoegaze" },
                    { 7, "💔", "Emo" },
                    { 8, "⚔️", "Metal" },
                    { 9, "🎷", "Jazz" },
                    { 10, "🪕", "Folk" },
                    { 11, "🎛️", "Electronic" },
                    { 12, "🎤", "Pop" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "City", "CreatedAt", "Email", "Latitude", "Longitude", "Name", "PasswordHash", "PhotoUrl", "Role", "RoleStatus" },
                values: new object[,]
                {
                    { 1, "Jakarta", new DateTime(2026, 9, 16, 2, 32, 28, 477, DateTimeKind.Utc).AddTicks(3449), "admin@gigradar.com", 0.0, 0.0, "Admin", "$2a$11$9ehs.zDkjVcvvGfOSC.YO.mNghWd65WTFeIL0fjkxe1rSuAqYktz.", "", "Admin", "None" },
                    { 2, "Jakarta", new DateTime(2026, 9, 16, 2, 32, 28, 650, DateTimeKind.Utc).AddTicks(3816), "eo@gigradar.com", 0.0, 0.0, "Event Organizer", "$2a$11$W52VCNIhewwZgsSn49MFOerm4XkndO3R2ECnrF/HQqnfgDgoL2PAS", "", "EO", "None" },
                    { 3, "Jakarta", new DateTime(2026, 9, 16, 2, 32, 28, 817, DateTimeKind.Utc).AddTicks(8268), "artist@gigradar.com", 0.0, 0.0, "Hollow Men", "$2a$11$tNRCUptaA.arz9JTjVi78ue8Hd5Cyz3y2WvbqRCgycit2uslV3H8S", "", "Artist", "None" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "VenueId", "Address", "Capacity", "City", "Latitude", "Longitude", "Name", "PhotoUrl" },
                values: new object[,]
                {
                    { 1, "Jl. Taman Ismail Marzuki", 1200, "Jakarta", -6.1943999999999999, 106.8329, "Graha Bhakti Budaya", "" },
                    { 2, "Jl. Sinabun", 500, "Bandung", -6.8849, 107.6143, "Gedung Kesenian", "" },
                    { 3, "Jl. Palmerah Selatan", 300, "Jakarta", -6.2297000000000002, 106.8184, "Bentara Budaya", "" }
                });

            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "ArtistId", "Bio", "City", "CoverUrl", "CreatedAt", "Genre", "Name", "PhotoUrl", "SocialLinks", "UserId" },
                values: new object[] { 1, "Shoegaze band dari Jakarta. Suara bising yang hangat, melodi yang melayang.", "Jakarta", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7553), "Shoegaze", "Hollow Men", "", "", 3 });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Capacity", "CreatedAt", "CreatedBy", "Description", "EndDate", "GenreId", "Latitude", "Longitude", "MaxPrice", "MinPrice", "Name", "PosterUrl", "SavesCount", "StartDate", "Status", "TicketLink", "VenueId", "ViewsCount" },
                values: new object[,]
                {
                    { 1, 200, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8356), 2, "Night penuh dengan suara Shoegaze dan Post-Punk", new DateTime(2026, 9, 5, 23, 0, 0, 0, DateTimeKind.Unspecified), 6, -6.2297000000000002, 106.8184, 35000m, 25000m, "Night of Shoegaze", "", 0, new DateTime(2026, 9, 5, 20, 0, 0, 0, DateTimeKind.Unspecified), "Published", "", 3, 0 },
                    { 2, 400, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8380), 1, "Festival emo terbesar di Bandung", new DateTime(2026, 9, 12, 23, 0, 0, 0, DateTimeKind.Unspecified), 7, -6.8849, 107.6143, 50000m, 30000m, "Midwest Emo Fest", "", 0, new DateTime(2026, 9, 12, 19, 0, 0, 0, DateTimeKind.Unspecified), "Published", "", 2, 0 },
                    { 3, 500, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8386), 1, "Indie night di tengah kota Jakarta", new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, -6.1943999999999999, 106.8329, 25000m, 20000m, "Underground Indie Night", "", 0, new DateTime(2026, 9, 19, 21, 0, 0, 0, DateTimeKind.Unspecified), "Published", "", 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "ArtistAlbums",
                columns: new[] { "AlbumId", "ArtistId", "CoverUrl", "CreatedAt", "Description", "ReleaseDate", "Title" },
                values: new object[] { 1, 1, "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7861), "Album pertama — 10 lagu shoegaze.", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pale Light" });

            migrationBuilder.InsertData(
                table: "ArtistJourneyItems",
                columns: new[] { "JourneyId", "ArtistId", "Category", "CreatedAt", "Date", "Description", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Formation", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8180), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hollow Men terbentuk di garasi Raka, Jakarta Selatan.", "", "Band terbentuk" },
                    { 2, 1, "Single", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8188), new DateTime(2025, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "'Static Haze' dirilis di platform digital.", "", "Single pertama" },
                    { 3, 1, "Gig", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8191), new DateTime(2025, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tampil di showcase underground pertama di Bentara Budaya.", "", "Showcase pertama" },
                    { 4, 1, "Album", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8193), new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Album perdana dirilis dengan 10 lagu.", "", "Album pertama: Pale Light" }
                });

            migrationBuilder.InsertData(
                table: "ArtistMembers",
                columns: new[] { "MemberId", "ArtistId", "CreatedAt", "JoinedAt", "Name", "PhotoUrl", "Role" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8096), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Raka", "", "Vokal & Gitar" },
                    { 2, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8104), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bima", "", "Gitar" },
                    { 3, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8106), new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sari", "", "Bass" },
                    { 4, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8109), new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dimas", "", "Drum" }
                });

            migrationBuilder.InsertData(
                table: "ArtistPosts",
                columns: new[] { "PostId", "ArtistId", "Content", "CreatedAt", "ImageUrl", "IsPublished", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Single terbaru kami 'Afterglow' sudah bisa didengarkan. Terima kasih yang sudah menunggu! 🌙", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "", true, "Single baru: Afterglow", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 1, "Kami akan tampil di Night of Shoegaze bulan September. Siapkan telingamu! 🎸", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", true, "Tampil di Night of Shoegaze", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "AudioTracks",
                columns: new[] { "TrackId", "ArtistId", "AudioUrl", "CoverUrl", "CreatedAt", "DurationSeconds", "Genre", "ReleaseDate", "Title" },
                values: new object[,]
                {
                    { 1, 1, "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7768), 210, "Shoegaze", new DateTime(2025, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Static Haze" },
                    { 2, 1, "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7784), 245, "Shoegaze", new DateTime(2025, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Midnight Bloom" },
                    { 3, 1, "", "", new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7788), 198, "Dream Pop", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Afterglow" }
                });

            migrationBuilder.InsertData(
                table: "EventArtists",
                columns: new[] { "Id", "ArtistId", "EventId", "Order" },
                values: new object[,]
                {
                    { 1, 1, 1, 1 },
                    { 2, 4, 1, 2 },
                    { 3, 2, 2, 1 },
                    { 4, 3, 2, 2 },
                    { 5, 5, 3, 1 },
                    { 6, 6, 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "EventTicketTypes",
                columns: new[] { "EventTicketTypeId", "Description", "EventId", "Name", "Price", "SortOrder", "Stock" },
                values: new object[,]
                {
                    { 1, "Akses penuh area festival (standing)", 1, "Festival", 35000m, 1, 150 },
                    { 2, "Area tribun duduk dengan pemandangan panggung", 1, "Tribun", 25000m, 2, 50 },
                    { 3, "Tiket festival + kaos eksklusif band", 1, "Bundling Festival + Merch", 60000m, 3, 30 },
                    { 4, "Akses penuh festival 1 hari", 2, "Festival", 50000m, 1, 300 },
                    { 5, "Area tribun duduk", 2, "Tribun", 30000m, 2, 100 },
                    { 6, "2 tiket festival dengan harga hemat", 2, "Bundling 2 Tiket", 90000m, 3, 40 },
                    { 7, "Akses penuh indie night", 3, "Festival", 25000m, 1, 350 },
                    { 8, "Area tribun duduk", 3, "Tribun", 20000m, 2, 150 },
                    { 9, "Tiket festival + 1 minuman gratis", 3, "Bundling Festival + Minuman", 35000m, 3, 60 }
                });

            migrationBuilder.InsertData(
                table: "Follows",
                columns: new[] { "FollowId", "ArtistId", "CreatedAt", "EventOrganizerId", "UserId", "VenueId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8256), null, 1, null },
                    { 2, 1, new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8259), null, 2, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAlbums_ArtistId",
                table: "ArtistAlbums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistJourneyItems_ArtistId",
                table: "ArtistJourneyItems",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMembers_ArtistId",
                table: "ArtistMembers",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistPosts_ArtistId",
                table: "ArtistPosts",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_UserId",
                table: "Artists",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AudioTracks_ArtistId",
                table: "AudioTracks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_EventArtists_ArtistId",
                table: "EventArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_EventArtists_EventId_ArtistId",
                table: "EventArtists",
                columns: new[] { "EventId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedBy",
                table: "Events",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Events_GenreId",
                table: "Events",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTicketTypes_EventId",
                table: "EventTicketTypes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_EventId",
                table: "Favorites",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_EventId",
                table: "Favorites",
                columns: new[] { "UserId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_ArtistId",
                table: "Follows",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_UserId_ArtistId",
                table: "Follows",
                columns: new[] { "UserId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_VenueId",
                table: "Follows",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRequests_ReviewedByAdminId",
                table: "RoleRequests",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleRequests_UserId",
                table: "RoleRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EventId",
                table: "Tickets",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_GenreId",
                table: "UserPreferences",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAlbums");

            migrationBuilder.DropTable(
                name: "ArtistJourneyItems");

            migrationBuilder.DropTable(
                name: "ArtistMembers");

            migrationBuilder.DropTable(
                name: "ArtistPosts");

            migrationBuilder.DropTable(
                name: "AudioTracks");

            migrationBuilder.DropTable(
                name: "EventArtists");

            migrationBuilder.DropTable(
                name: "EventTicketTypes");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropTable(
                name: "RoleRequests");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
