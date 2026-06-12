using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Registry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetBelongings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Belongings",
                schema: "registry",
                table: "pets",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Belongings",
                schema: "registry",
                table: "pets");
        }
    }
}
