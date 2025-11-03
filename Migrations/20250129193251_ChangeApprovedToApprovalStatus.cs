using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeApprovedToApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "DriverLeaves");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DriverLeaves",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DriverLeaves");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "DriverLeaves",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
