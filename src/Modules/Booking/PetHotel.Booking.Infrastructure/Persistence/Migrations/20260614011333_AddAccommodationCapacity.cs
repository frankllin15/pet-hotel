using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                schema: "booking",
                table: "accommodations",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                schema: "booking",
                table: "accommodations");
        }
    }
}
