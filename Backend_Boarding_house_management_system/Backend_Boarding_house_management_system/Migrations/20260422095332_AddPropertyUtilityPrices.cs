using Backend_Boarding_house_management_system.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Boarding_house_management_system.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260422095332_AddPropertyUtilityPrices")]
    public partial class AddPropertyUtilityPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ElectricPrice",
                table: "Properties",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterPrice",
                table: "Properties",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricPrice",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "WaterPrice",
                table: "Properties");
        }
    }
}
