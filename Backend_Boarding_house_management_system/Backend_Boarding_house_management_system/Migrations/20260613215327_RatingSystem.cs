using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Boarding_house_management_system.Migrations
{
    /// <inheritdoc />
    public partial class RatingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyAspectScores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PropertyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Aspect = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PositiveCount = table.Column<int>(type: "integer", nullable: false),
                    NegativeCount = table.Column<int>(type: "integer", nullable: false),
                    NeutralCount = table.Column<int>(type: "integer", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    WeightedScore = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAspectScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyAspectScores_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RatingAspects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RatingId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Aspect = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sentiment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingAspects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingAspects_Ratings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "Ratings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAspectScores_PropertyId_Aspect",
                table: "PropertyAspectScores",
                columns: new[] { "PropertyId", "Aspect" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RatingAspects_RatingId",
                table: "RatingAspects",
                column: "RatingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyAspectScores");

            migrationBuilder.DropTable(
                name: "RatingAspects");
        }
    }
}
