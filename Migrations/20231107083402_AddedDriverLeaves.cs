using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedDriverLeaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverLeaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLeaves", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverLeaves");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Chats");
        }
    }
}
