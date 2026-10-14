using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RouteInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WeatherInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Users_OrganiserId",
                        column: x => x.OrganiserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    RaceNumber = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.EnrolmentId);
                    table.ForeignKey(
                        name: "FK_Enrolments_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrolments_Users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    FinishTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: true),
                    Pace = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_Results_Enrolments_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolments",
                        principalColumn: "EnrolmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "john@raceday.co.za", "John Van Der Merwe", "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02", "Organiser" },
                    { 2, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "sarah@raceday.co.za", "Sarah Naidoo", "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02", "Organiser" },
                    { 3, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "thabo@gmail.com", "Thabo Mokoena", "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02", "Participant" },
                    { 4, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "lisa@gmail.com", "Lisa Van Wyk", "$2a$11$M3xr3VgmFYsZl8sec2VxwuTAKzADJs2r/boxAzs4voGHBO5DkKe02", "Participant" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "CreatedAt", "Description", "Distance", "EventDate", "EventType", "IsActive", "Location", "OrganiserId", "RouteInfo", "Title", "WeatherInfo" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6762), "The Ultimate Human Race - 90km", 90m, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "run", true, "Pietermaritzburg to Durban", 1, null, "Comrades Marathon 2026", null },
                    { 2, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6786), "World's largest timed cycle race", 109m, new DateTime(2026, 3, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "cycle", true, "Cape Town", 1, null, "Cape Town Cycle Tour 2026", null },
                    { 3, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6789), "World's most beautiful marathon", 56m, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "run", true, "Cape Town", 2, null, "Two Oceans Marathon 2026", null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CreatedAt", "DistanceKm", "EntryFee", "EventId", "MaxParticipants", "Name", "StartTime" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6840), 90m, 800m, 1, 5000, "Open Men (Ultra)", new TimeOnly(6, 0, 0) },
                    { 2, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6853), 90m, 800m, 1, 4500, "Open Women (Ultra)", new TimeOnly(6, 0, 0) },
                    { 3, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6856), 109m, 700m, 2, 10000, "Open Men (Cycle)", new TimeOnly(6, 30, 0) },
                    { 4, new DateTime(2026, 10, 14, 9, 45, 41, 81, DateTimeKind.Utc).AddTicks(6858), 56m, 950m, 3, 3000, "Ultra Marathon Men", new TimeOnly(5, 45, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_EventId",
                table: "Categories",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_CategoryId",
                table: "Enrolments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ParticipantId_CategoryId",
                table: "Enrolments",
                columns: new[] { "ParticipantId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganiserId",
                table: "Events",
                column: "OrganiserId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_EnrolmentId",
                table: "Results",
                column: "EnrolmentId",
                unique: true);

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
                name: "Results");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
