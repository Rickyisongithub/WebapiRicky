using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedRowModInJourneys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowMod",
                table: "TruckLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowMod",
                table: "JourneyTrucks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowMod",
                table: "Journeys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowMod",
                table: "JourneyDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowMod",
                table: "TruckLocations");

            migrationBuilder.DropColumn(
                name: "RowMod",
                table: "JourneyTrucks");

            migrationBuilder.DropColumn(
                name: "RowMod",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "RowMod",
                table: "JourneyDetails");
        }
    }
}
