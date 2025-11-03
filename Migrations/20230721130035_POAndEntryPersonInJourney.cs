using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class POAndEntryPersonInJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntryPerson",
                table: "Journeys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PONum",
                table: "Journeys",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryPerson",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "PONum",
                table: "Journeys");
        }
    }
}
