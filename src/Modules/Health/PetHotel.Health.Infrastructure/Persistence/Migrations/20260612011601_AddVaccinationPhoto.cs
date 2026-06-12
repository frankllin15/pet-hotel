using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Health.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoKey",
                schema: "health",
                table: "vaccinations",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoKey",
                schema: "health",
                table: "vaccinations");
        }
    }
}
