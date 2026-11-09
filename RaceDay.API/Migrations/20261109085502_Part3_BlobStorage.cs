using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class Part3_BlobStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9833));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9892));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9894));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9896));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                columns: new[] { "BannerUrl", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9759) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                columns: new[] { "BannerUrl", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9784) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                columns: new[] { "BannerUrl", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 11, 9, 8, 55, 0, 617, DateTimeKind.Utc).AddTicks(9787) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "PasswordHash", "ProfilePictureUrl" },
                values: new object[] { "$2a$11$hrrmq1HwccQsIDFVk.S5nu.MuFn9TzB3QUQyxyM7d2ugEqipI/rfq", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "PasswordHash", "ProfilePictureUrl" },
                values: new object[] { "$2a$11$hrrmq1HwccQsIDFVk.S5nu.MuFn9TzB3QUQyxyM7d2ugEqipI/rfq", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "PasswordHash", "ProfilePictureUrl" },
                values: new object[] { "$2a$11$hrrmq1HwccQsIDFVk.S5nu.MuFn9TzB3QUQyxyM7d2ugEqipI/rfq", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                columns: new[] { "PasswordHash", "ProfilePictureUrl" },
                values: new object[] { "$2a$11$hrrmq1HwccQsIDFVk.S5nu.MuFn9TzB3QUQyxyM7d2ugEqipI/rfq", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6840));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6853));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6856));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6858));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6762));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6786));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6789));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02");
        }
    }
}
