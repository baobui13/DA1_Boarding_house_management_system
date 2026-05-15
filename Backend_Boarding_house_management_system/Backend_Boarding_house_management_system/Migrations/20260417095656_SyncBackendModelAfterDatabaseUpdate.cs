using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Boarding_house_management_system.Migrations
{
    /// <inheritdoc />
    public partial class SyncBackendModelAfterDatabaseUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Appointments" DROP CONSTRAINT IF EXISTS "FK_Appointments_Properties_RoomId";""");
            migrationBuilder.Sql("""ALTER TABLE "Appointments" DROP CONSTRAINT IF EXISTS "FK_Appointments_Users_UserId";""");
            migrationBuilder.Sql("""ALTER TABLE "Contracts" DROP CONSTRAINT IF EXISTS "FK_Contracts_Properties_RoomId";""");
            migrationBuilder.Sql("""ALTER TABLE "Contracts" DROP CONSTRAINT IF EXISTS "FK_Contracts_Users_TenantId";""");
            migrationBuilder.Sql("""ALTER TABLE "Messages" DROP CONSTRAINT IF EXISTS "FK_Messages_Contracts_ContractId";""");
            migrationBuilder.Sql("""ALTER TABLE "Messages" DROP CONSTRAINT IF EXISTS "FK_Messages_Properties_RoomId";""");
            migrationBuilder.Sql("""ALTER TABLE "RoomAmenities" DROP CONSTRAINT IF EXISTS "FK_RoomAmenities_Properties_RoomId";""");
            migrationBuilder.Sql("""ALTER TABLE "ViewHistories" DROP CONSTRAINT IF EXISTS "FK_ViewHistories_Properties_RoomId";""");

            RenameColumnIfExists(migrationBuilder, "ViewHistories", "RoomId", "PropertyId");
            RenameIndexIfExists(migrationBuilder, "IX_ViewHistories_RoomId", "IX_ViewHistories_PropertyId");

            RenameColumnIfExists(migrationBuilder, "RoomAmenities", "RoomId", "PropertyId");
            RenameIndexIfExists(migrationBuilder, "IX_RoomAmenities_RoomId", "IX_RoomAmenities_PropertyId");

            RenameColumnIfExists(migrationBuilder, "Messages", "RoomId", "PropertyId");
            RenameIndexIfExists(migrationBuilder, "IX_Messages_RoomId", "IX_Messages_PropertyId");

            RenameColumnIfExists(migrationBuilder, "Invoices", "WaterUsage", "OldWaterReading");
            RenameColumnIfExists(migrationBuilder, "Invoices", "ElectricityUsage", "OldElectricityReading");

            RenameColumnIfExists(migrationBuilder, "Contracts", "RoomId", "PropertyId");
            RenameIndexIfExists(migrationBuilder, "IX_Contracts_RoomId", "IX_Contracts_PropertyId");

            RenameColumnIfExists(migrationBuilder, "Appointments", "RoomId", "PropertyId");
            RenameIndexIfExists(migrationBuilder, "IX_Appointments_RoomId", "IX_Appointments_PropertyId");

            AddColumnIfNotExists(
                migrationBuilder,
                "Users",
                "CCCD",
                "character varying(20) NOT NULL DEFAULT ''");

            AddColumnIfNotExists(
                migrationBuilder,
                "Users",
                "ReputationScore",
                "integer NOT NULL DEFAULT 0");

            AddColumnIfNotExists(
                migrationBuilder,
                "Invoices",
                "NewElectricityReading",
                "numeric(10,2) NULL");

            AddColumnIfNotExists(
                migrationBuilder,
                "Invoices",
                "NewWaterReading",
                "numeric(10,2) NULL");

            AddColumnIfNotExists(
                migrationBuilder,
                "Invoices",
                "ReceiptUrl",
                "character varying(255) NULL");

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Complaints" (
                    "Id" character varying(50) NOT NULL,
                    "CreatorId" character varying(50) NOT NULL,
                    "RelatedType" character varying(50) NOT NULL,
                    "RelatedId" character varying(50) NOT NULL,
                    "Title" character varying(200) NOT NULL,
                    "Content" text NOT NULL,
                    "Status" character varying(50) NOT NULL,
                    "AdminResponse" text NULL,
                    "ResolvedAt" timestamp with time zone NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_Complaints" PRIMARY KEY ("Id")
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Ratings" (
                    "Id" character varying(50) NOT NULL,
                    "TenantId" character varying(50) NOT NULL,
                    "PropertyId" character varying(50) NOT NULL,
                    "Stars" integer NOT NULL,
                    "Content" text NOT NULL,
                    "AIAttitude" character varying(50) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_Ratings" PRIMARY KEY ("Id")
                );
            """);

            CreateIndexIfNotExists(migrationBuilder, "IX_Complaints_CreatorId", "Complaints", "CreatorId");
            CreateIndexIfNotExists(migrationBuilder, "IX_Ratings_PropertyId", "Ratings", "PropertyId");
            CreateIndexIfNotExists(migrationBuilder, "IX_Ratings_TenantId", "Ratings", "TenantId");

            AddForeignKeyIfNotExists(migrationBuilder, "FK_Appointments_Properties_PropertyId", "Appointments", "PropertyId", "Properties", "Id", "CASCADE");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Appointments_Users_UserId", "Appointments", "UserId", "Users", "Id", "RESTRICT");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Contracts_Properties_PropertyId", "Contracts", "PropertyId", "Properties", "Id", "RESTRICT");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Contracts_Users_TenantId", "Contracts", "TenantId", "Users", "Id", "RESTRICT");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Messages_Contracts_ContractId", "Messages", "ContractId", "Contracts", "Id", "SET NULL");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Messages_Properties_PropertyId", "Messages", "PropertyId", "Properties", "Id", "SET NULL");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_RoomAmenities_Properties_PropertyId", "RoomAmenities", "PropertyId", "Properties", "Id", "CASCADE");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_ViewHistories_Properties_PropertyId", "ViewHistories", "PropertyId", "Properties", "Id", "CASCADE");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Complaints_Users_CreatorId", "Complaints", "CreatorId", "Users", "Id", "RESTRICT");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Ratings_Properties_PropertyId", "Ratings", "PropertyId", "Properties", "Id", "CASCADE");
            AddForeignKeyIfNotExists(migrationBuilder, "FK_Ratings_Users_TenantId", "Ratings", "TenantId", "Users", "Id", "RESTRICT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Properties_PropertyId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_UserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Properties_PropertyId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_TenantId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Contracts_ContractId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Properties_PropertyId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomAmenities_Properties_PropertyId",
                table: "RoomAmenities");

            migrationBuilder.DropForeignKey(
                name: "FK_ViewHistories_Properties_PropertyId",
                table: "ViewHistories");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropColumn(
                name: "CCCD",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReputationScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NewElectricityReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "NewWaterReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "ViewHistories",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_ViewHistories_PropertyId",
                table: "ViewHistories",
                newName: "IX_ViewHistories_RoomId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "RoomAmenities",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_RoomAmenities_PropertyId",
                table: "RoomAmenities",
                newName: "IX_RoomAmenities_RoomId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Messages",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_PropertyId",
                table: "Messages",
                newName: "IX_Messages_RoomId");

            migrationBuilder.RenameColumn(
                name: "OldWaterReading",
                table: "Invoices",
                newName: "WaterUsage");

            migrationBuilder.RenameColumn(
                name: "OldElectricityReading",
                table: "Invoices",
                newName: "ElectricityUsage");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Contracts",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_PropertyId",
                table: "Contracts",
                newName: "IX_Contracts_RoomId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "Appointments",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PropertyId",
                table: "Appointments",
                newName: "IX_Appointments_RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Properties_RoomId",
                table: "Appointments",
                column: "RoomId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_UserId",
                table: "Appointments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Properties_RoomId",
                table: "Contracts",
                column: "RoomId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_TenantId",
                table: "Contracts",
                column: "TenantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Contracts_ContractId",
                table: "Messages",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Properties_RoomId",
                table: "Messages",
                column: "RoomId",
                principalTable: "Properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAmenities_Properties_RoomId",
                table: "RoomAmenities",
                column: "RoomId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ViewHistories_Properties_RoomId",
                table: "ViewHistories",
                column: "RoomId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        private static void RenameColumnIfExists(MigrationBuilder migrationBuilder, string table, string oldName, string newName)
        {
            migrationBuilder.Sql($$"""
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = '{{table}}'
                          AND column_name = '{{oldName}}'
                    ) AND NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = '{{table}}'
                          AND column_name = '{{newName}}'
                    ) THEN
                        EXECUTE 'ALTER TABLE "{{table}}" RENAME COLUMN "{{oldName}}" TO "{{newName}}"';
                    END IF;
                END
                $migration$;
            """);
        }

        private static void RenameIndexIfExists(MigrationBuilder migrationBuilder, string oldName, string newName)
        {
            migrationBuilder.Sql($$"""
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_class
                        WHERE relkind = 'i'
                          AND relname = '{{oldName}}'
                    ) AND NOT EXISTS (
                        SELECT 1
                        FROM pg_class
                        WHERE relkind = 'i'
                          AND relname = '{{newName}}'
                    ) THEN
                        EXECUTE 'ALTER INDEX "{{oldName}}" RENAME TO "{{newName}}"';
                    END IF;
                END
                $migration$;
            """);
        }

        private static void AddColumnIfNotExists(MigrationBuilder migrationBuilder, string table, string column, string columnDefinition)
        {
            migrationBuilder.Sql($$"""
                ALTER TABLE "{{table}}"
                ADD COLUMN IF NOT EXISTS "{{column}}" {{columnDefinition}};
            """);
        }

        private static void CreateIndexIfNotExists(MigrationBuilder migrationBuilder, string indexName, string table, string column)
        {
            migrationBuilder.Sql($$"""
                CREATE INDEX IF NOT EXISTS "{{indexName}}" ON "{{table}}" ("{{column}}");
            """);
        }

        private static void AddForeignKeyIfNotExists(
            MigrationBuilder migrationBuilder,
            string constraintName,
            string table,
            string column,
            string principalTable,
            string principalColumn,
            string onDelete)
        {
            migrationBuilder.Sql($$"""
                DO $migration$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = '{{constraintName}}'
                    ) THEN
                        EXECUTE 'ALTER TABLE "{{table}}" ADD CONSTRAINT "{{constraintName}}" FOREIGN KEY ("{{column}}") REFERENCES "{{principalTable}}" ("{{principalColumn}}") ON DELETE {{onDelete}}';
                    END IF;
                END
                $migration$;
            """);
        }
    }
}
