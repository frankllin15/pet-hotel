using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Health.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParasitesAndVetContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VetContact",
                schema: "health",
                table: "health_records",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "parasite_treatments",
                schema: "health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AppliedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDueOn = table.Column<DateOnly>(type: "date", nullable: true),
                    HealthRecordId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parasite_treatments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parasite_treatments_health_records_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalSchema: "health",
                        principalTable: "health_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parasite_treatments_HealthRecordId",
                schema: "health",
                table: "parasite_treatments",
                column: "HealthRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parasite_treatments",
                schema: "health");

            migrationBuilder.DropColumn(
                name: "VetContact",
                schema: "health",
                table: "health_records");
        }
    }
}
