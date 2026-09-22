using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationMerchReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminUserId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "Merchandises",
                columns: table => new
                {
                    MerchandiseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    ExternalPurchaseUrl = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchandises", x => x.MerchandiseId);
                    table.ForeignKey(
                        name: "FK_Merchandises_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReporterUserId = table.Column<int>(type: "integer", nullable: false),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ModerationNote = table.Column<string>(type: "text", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8819));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9122));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9133));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9138));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9140));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8997));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9001));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9003));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9005));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8071));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8172));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8177));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8189));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8193));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8343));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8704));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8718));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(8721));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9420));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9443));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9451));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9281));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 16, 36, 647, DateTimeKind.Utc).AddTicks(9285));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 16, 35, 977, DateTimeKind.Utc).AddTicks(504), "$2a$11$y5CtD1YivlZcwoUcGGSEd..XRS51oIJQRevfvHVZA0S0ZpOU6PjL2" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 16, 36, 172, DateTimeKind.Utc).AddTicks(5229), "$2a$11$hiSnyu6GIHPvUNtB5PlDwuBqQPfgIf8WaPM94SAvYPitg6PXjSARu" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 16, 36, 419, DateTimeKind.Utc).AddTicks(3133), "$2a$11$y3jwa50mP3gmoHEgATDFbeqM8diHazbp9xJ22ApCzYFjMfa9ytXEe" });

            migrationBuilder.CreateIndex(
                name: "IX_Merchandises_ArtistId",
                table: "Merchandises",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterUserId",
                table: "Reports",
                column: "ReporterUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Merchandises");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4006));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4199));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4205));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4207));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4208));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4140));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4146));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4148));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4149));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3690));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3761));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3765));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3771));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3872));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3875));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3946));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(3962));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4322));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4343));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4346));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4244));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 9, 56, 225, DateTimeKind.Utc).AddTicks(4246));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 9, 55, 690, DateTimeKind.Utc).AddTicks(7662), "$2a$11$YDTRkYg4MNTCnmAPxNQ9VeZKRJZ7ZWAsczNy9.Jv5u6eeyvel36y." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 9, 55, 850, DateTimeKind.Utc).AddTicks(8283), "$2a$11$.kFFCE/Jm9amzO8ovJlHouPk21mQBVf.Ds/j2O95wNWTjB4T6N33m" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 9, 56, 23, DateTimeKind.Utc).AddTicks(2612), "$2a$11$P/9dUZo1utAGyHjOonwvY.T98lkSyrIgNbC6N9yDhUoYtKiFluLUW" });
        }
    }
}
