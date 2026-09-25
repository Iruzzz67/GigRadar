using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GigRadarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityAndShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotifyUrl",
                table: "Artists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommunityPosts",
                columns: table => new
                {
                    CommunityPostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityPosts", x => x.CommunityPostId);
                    table.ForeignKey(
                        name: "FK_CommunityPosts_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityPosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Artists_ArtistId",
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
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(958));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1132));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1137));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1139));

            migrationBuilder.UpdateData(
                table: "ArtistJourneyItems",
                keyColumn: "JourneyId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1141));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1084));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1089));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1091));

            migrationBuilder.UpdateData(
                table: "ArtistMembers",
                keyColumn: "MemberId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1092));

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(670), "" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(734), "" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(737), "" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(833), "" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(836), "" });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "ArtistId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "SpotifyUrl" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(837), "" });

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(901));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(911));

            migrationBuilder.UpdateData(
                table: "AudioTracks",
                keyColumn: "TrackId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(913));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1251));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1272));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1276));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Follows",
                keyColumn: "FollowId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 25, 4, 46, 49, 496, DateTimeKind.Utc).AddTicks(1183));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 66, DateTimeKind.Utc).AddTicks(6394), "$2a$11$Q2BO.V1keKV7O0BsFqbXYOz9wv3O81k9QXjBCHi9nj3nBd3guHsqy" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 219, DateTimeKind.Utc).AddTicks(8112), "$2a$11$Wm6VTiVx4ErFVy/R9VjYwegPPvYG8hT/QP9SYW9Hu.91DF59W31Fq" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 9, 25, 4, 46, 49, 359, DateTimeKind.Utc).AddTicks(1048), "$2a$11$asFW43gZlQOGKx1CxLvHaun2XiqarHUdgAXDM9OrL2UQ.NCv1ZpvS" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_ArtistId",
                table: "CommunityPosts",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_UserId",
                table: "CommunityPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ArtistId",
                table: "Products",
                column: "ArtistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityPosts");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropColumn(
                name: "SpotifyUrl",
                table: "Artists");

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
        }
    }
}
