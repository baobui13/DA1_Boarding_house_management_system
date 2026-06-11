using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Boarding_house_management_system.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryUserTimestampIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ViewHistories_UserId",
                table: "ViewHistories");

            migrationBuilder.DropIndex(
                name: "IX_SearchHistories_UserId",
                table: "SearchHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ViewHistories_UserId_Timestamp",
                table: "ViewHistories",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistories_UserId_Timestamp",
                table: "SearchHistories",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ViewHistories_UserId_Timestamp",
                table: "ViewHistories");

            migrationBuilder.DropIndex(
                name: "IX_SearchHistories_UserId_Timestamp",
                table: "SearchHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ViewHistories_UserId",
                table: "ViewHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistories_UserId",
                table: "SearchHistories",
                column: "UserId");
        }
    }
}
