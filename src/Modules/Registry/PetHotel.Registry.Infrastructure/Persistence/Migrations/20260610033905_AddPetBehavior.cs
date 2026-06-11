using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Registry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BehaviorNotes",
                schema: "registry",
                table: "pets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destructiveness",
                schema: "registry",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fear",
                schema: "registry",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reactivity",
                schema: "registry",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sociability",
                schema: "registry",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BehaviorNotes",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Destructiveness",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Fear",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Reactivity",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Sociability",
                schema: "registry",
                table: "pets");
        }
    }
}
