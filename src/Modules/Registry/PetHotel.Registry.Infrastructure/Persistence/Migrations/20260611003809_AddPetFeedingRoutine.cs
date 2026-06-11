using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Registry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetFeedingRoutine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedingRoutine",
                schema: "registry",
                table: "pets",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedingRoutine",
                schema: "registry",
                table: "pets");
        }
    }
}
