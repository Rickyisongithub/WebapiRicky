using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverLeaveBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Approved",
                table: "DriverLeaves",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateTable(
                name: "DriverLeaveBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalLeaves = table.Column<int>(type: "int", nullable: false),
                    UsedLeaves = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLeaveBalances", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverLeaveBalances");

            migrationBuilder.AlterColumn<bool>(
                name: "Approved",
                table: "DriverLeaves",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
