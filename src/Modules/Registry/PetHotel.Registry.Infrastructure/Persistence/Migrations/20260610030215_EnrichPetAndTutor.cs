using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Registry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichPetAndTutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPickups",
                schema: "registry",
                table: "tutors",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContacts",
                schema: "registry",
                table: "tutors",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrochipCode",
                schema: "registry",
                table: "pets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Neutered",
                schema: "registry",
                table: "pets",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                schema: "registry",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                schema: "registry",
                table: "pets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizedPickups",
                schema: "registry",
                table: "tutors");

            migrationBuilder.DropColumn(
                name: "EmergencyContacts",
                schema: "registry",
                table: "tutors");

            migrationBuilder.DropColumn(
                name: "MicrochipCode",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Neutered",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Sex",
                schema: "registry",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "Size",
                schema: "registry",
                table: "pets");
        }
    }
}
