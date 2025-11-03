using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TravelStartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TravelEndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Hours = table.Column<int>(type: "int", nullable: true),
                    OrderNum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustNum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NeedByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JourneyDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hours = table.Column<int>(type: "int", nullable: true),
                    JourneyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyDetails_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyDetails_JourneyId",
                table: "JourneyDetails",
                column: "JourneyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JourneyDetails");

            migrationBuilder.DropTable(
                name: "Journeys");
        }
    }
}
