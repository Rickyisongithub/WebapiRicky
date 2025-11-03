using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedDaysinDriverLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Days",
                table: "DriverLeaves",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Days",
                table: "DriverLeaves");
        }
    }
}
