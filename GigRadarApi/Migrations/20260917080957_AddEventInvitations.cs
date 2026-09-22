using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEventInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventInvitations",
                columns: table => new
                {
                    InvitationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInvitations", x => x.InvitationId);
                    table.ForeignKey(
                        name: "FK_EventInvitations_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventInvitations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_ArtistId",
                table: "EventInvitations",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_EventId_ArtistId_Status",
                table: "EventInvitations",
                columns: new[] { "EventId", "ArtistId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInvitations");

            migrationBuilder.UpdateData(
                table: "ArtistAlbums",
                keyColumn: "AlbumId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7996));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8177));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8181));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8182));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8184));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8101));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8105));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8106));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8107));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7699));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7759));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7762));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7858));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7861));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7863));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7942));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7956));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(7958));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8286));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8297));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8300));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8221));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 17, 8, 7, 54, 939, DateTimeKind.Utc).AddTicks(8223));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 7, 54, 409, DateTimeKind.Utc).AddTicks(5356), "$2a$11$Sy79WAUsHyD.PfTYtelIzeWf5syB8H2V7ndkVRKguXEtzXvA945o6" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 7, 54, 551, DateTimeKind.Utc).AddTicks(6920), "$2a$11$C5E7b6VjQ0EgWSpRvwF/juyKpq.mbjsJrFg/S3kW5E5/Yu6Tc4D22" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 7, 54, 793, DateTimeKind.Utc).AddTicks(3499), "$2a$11$yIJgfBXeFrMBobYUK15TWeUUq2EdysxVUytx4FFpe04xWLLeiWaBC" });
        }
    }
}
