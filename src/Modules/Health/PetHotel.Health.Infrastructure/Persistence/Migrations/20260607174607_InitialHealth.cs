using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Health.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "health");

            migrationBuilder.CreateTable(
                name: "health_records",
                schema: "health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vaccinations",
                schema: "health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AppliedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: false),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vaccinations_health_records_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalSchema: "health",
                        principalTable: "health_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_health_records_TenantId_pet_id",
                schema: "health",
                table: "health_records",
                columns: new[] { "TenantId", "pet_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaccinations_HealthRecordId",
                schema: "health",
                table: "vaccinations",
                column: "HealthRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vaccinations",
                schema: "health");

            migrationBuilder.DropTable(
                name: "health_records",
                schema: "health");
        }
    }
}
