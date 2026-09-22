using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizers",
                columns: table => new
                {
                    OrganizerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: false),
                    SocialLinks = table.Column<string>(type: "text", nullable: false),
                    FoundedYear = table.Column<int>(type: "integer", nullable: true),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizers", x => x.OrganizerId);
                    table.ForeignKey(
                        name: "FK_Organizers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "OrganizerPortfolioItems",
                columns: table => new
                {
                    PortfolioItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizerId = table.Column<int>(type: "integer", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Venue = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PosterUrl = table.Column<string>(type: "text", nullable: false),
                    PhotosUrl = table.Column<string>(type: "text", nullable: false),
                    DocumentationLink = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerPortfolioItems", x => x.PortfolioItemId);
                    table.ForeignKey(
                        name: "FK_OrganizerPortfolioItems_Organizers_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Organizers",
                        principalColumn: "OrganizerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizerVerifications",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizerId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: false),
                    PortfolioCount = table.Column<int>(type: "integer", nullable: false),
                    PortfolioSummary = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RejectedReason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizerVerifications", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_OrganizerVerifications_Organizers_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Organizers",
                        principalColumn: "OrganizerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3395));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3661));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3721));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3723));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3725));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3586));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3589));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3591));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3593));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(2970));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3051));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3055));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3062));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3184));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3190));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3307));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3320));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3324));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3974));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3997));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(4002));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3811));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 7, 45, 15, 842, DateTimeKind.Utc).AddTicks(3814));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 7, 45, 15, 377, DateTimeKind.Utc).AddTicks(9678), "$2a$11$6su2KGJzQN8j3SxuGQE7LejVL1rb33pt/tmg4VxIebA3p.4rmY47u" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 7, 45, 15, 516, DateTimeKind.Utc).AddTicks(3762), "$2a$11$Shh7veBhESqYQ8Mc2KKfm.WFf8argVpoXarnmclvAyVHpxV6np4He" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 7, 45, 15, 677, DateTimeKind.Utc).AddTicks(8536), "$2a$11$3n377vmuMH/k9wMaXeiHDu6JvPAjWK2eWDeoLcT5KyAndCGeeUYcO" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerPortfolioItems_OrganizerId",
                table: "OrganizerPortfolioItems",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizers_UserId",
                table: "Organizers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizerVerifications_OrganizerId_Status",
                table: "OrganizerVerifications",
                columns: new[] { "OrganizerId", "Status" },
                filter: "\"Status\" = 'PENDING'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerPortfolioItems");

            migrationBuilder.DropTable(
                name: "OrganizerVerifications");

            migrationBuilder.DropTable(
                name: "Organizers");

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2034));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2256));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2260));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2262));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2263));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2205));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2208));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2210));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2211));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1808));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1873));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1876));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1881));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1884));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1886));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1970));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1980));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1982));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2398));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2415));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2303));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(2306));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 38, 954, DateTimeKind.Utc).AddTicks(7826), "$2a$11$IjMxs/zA.7kHl26TFo349.cF3UgaZRhrSEhWYYwMqc14VJ7B8B.h6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 124, DateTimeKind.Utc).AddTicks(5032), "$2a$11$orpqE5bCxskQOo34y8HIsOPyl9L0N7x1BkW049d5tbCdP4RZXQyWG" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 307, DateTimeKind.Utc).AddTicks(1187), "$2a$11$UmWQ8ea5Jic2.9atuQHir.Ryt5X5fs5nMAiJHzvtV1ZJY4zgTSqiO" });
        }
    }
}
