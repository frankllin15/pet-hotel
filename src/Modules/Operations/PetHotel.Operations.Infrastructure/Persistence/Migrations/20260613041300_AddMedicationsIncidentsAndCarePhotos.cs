using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationsIncidentsAndCarePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "PhotoKeys",
                schema: "operations",
                table: "care_log_entries",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'"); // linhas existentes recebem array vazio

            migrationBuilder.CreateTable(
                name: "incidents",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContextType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medications",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContextType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Drug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Dose = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AdministeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_incidents_TenantId_context_id_OccurredAt",
                schema: "operations",
                table: "incidents",
                columns: new[] { "TenantId", "context_id", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_medications_TenantId_context_id_AdministeredAt",
                schema: "operations",
                table: "medications",
                columns: new[] { "TenantId", "context_id", "AdministeredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "incidents",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "medications",
                schema: "operations");

            migrationBuilder.DropColumn(
                name: "PhotoKeys",
                schema: "operations",
                table: "care_log_entries");
        }
    }
}
