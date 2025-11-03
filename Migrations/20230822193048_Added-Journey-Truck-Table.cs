using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedJourneyTruckTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JourneyTrucks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JourneyId = table.Column<long>(type: "bigint", nullable: false),
                    TruckId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hours = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyTrucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyTrucks_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TruckLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JourneyTruckId = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hours = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckLocations_JourneyTrucks_JourneyTruckId",
                        column: x => x.JourneyTruckId,
                        principalTable: "JourneyTrucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyTrucks_JourneyId",
                table: "JourneyTrucks",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocations_JourneyTruckId",
                table: "TruckLocations",
                column: "JourneyTruckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TruckLocations");

            migrationBuilder.DropTable(
                name: "JourneyTrucks");
        }
    }
}
