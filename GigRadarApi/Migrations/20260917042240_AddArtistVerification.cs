using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Artists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "Artists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ArtistVerifications",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    LegalName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    SpotifyUrl = table.Column<string>(type: "text", nullable: false),
                    SupportingInfo = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RejectedReason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistVerifications", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_ArtistVerifications_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1808), false, "NOT_SUBMITTED" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1873), false, "NOT_SUBMITTED" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1876), false, "NOT_SUBMITTED" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1881), false, "NOT_SUBMITTED" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1884), false, "NOT_SUBMITTED" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "IsVerified", "VerificationStatus" },
                values: new object[] { new DateTime(2026, 9, 17, 4, 22, 39, 448, DateTimeKind.Utc).AddTicks(1886), false, "NOT_SUBMITTED" });

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

            migrationBuilder.CreateIndex(
                name: "IX_ArtistVerifications_ArtistId_Status",
                table: "ArtistVerifications",
                columns: new[] { "ArtistId", "Status" },
                filter: "\"Status\" = 'PENDING'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Kolom Artists.IsVerified/VerificationStatus NOT NULL direferensikan FK tabel
            // ArtistVerifications, jadi tabel harus dihapus lebih dulu agar DropColumn tidak gagal.
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Artists");

            migrationBuilder.DropTable(
                name: "ArtistVerifications");

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7861));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8180));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8188));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8191));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8193));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8096));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8104));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8109));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7553));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7631));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7637));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7645));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7649));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7652));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7768));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7784));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(7788));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8356));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8380));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8386));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8256));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 16, 2, 32, 29, 15, DateTimeKind.Utc).AddTicks(8259));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 16, 2, 32, 28, 477, DateTimeKind.Utc).AddTicks(3449), "$2a$11$9ehs.zDkjVcvvGfOSC.YO.mNghWd65WTFeIL0fjkxe1rSuAqYktz." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 16, 2, 32, 28, 650, DateTimeKind.Utc).AddTicks(3816), "$2a$11$W52VCNIhewwZgsSn49MFOerm4XkndO3R2ECnrF/HQqnfgDgoL2PAS" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 16, 2, 32, 28, 817, DateTimeKind.Utc).AddTicks(8268), "$2a$11$tNRCUptaA.arz9JTjVi78ue8Hd5Cyz3y2WvbqRCgycit2uslV3H8S" });
        }
    }
}
