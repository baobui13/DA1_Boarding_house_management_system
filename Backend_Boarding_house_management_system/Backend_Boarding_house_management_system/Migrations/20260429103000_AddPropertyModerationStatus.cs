using Backend_Boarding_house_management_system.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Boarding_house_management_system.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260429103000_AddPropertyModerationStatus")]
    public partial class AddPropertyModerationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Properties"
                ADD COLUMN IF NOT EXISTS "ModerationStatus" integer NOT NULL DEFAULT 0;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Properties"
                DROP COLUMN IF EXISTS "ModerationStatus";
            """);
        }
    }
}
