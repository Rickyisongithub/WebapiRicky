using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairosWebAPI.Migrations
{
    /// <inheritdoc />
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    public partial class addedtrucktable : Migration
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Part_Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part_PartNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part_PartDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part_UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Part_SalesUM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commission = table.Column<float>(type: "real", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trucks");
        }
    }
}
