using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationArrivalState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrivalState",
                schema: "booking",
                table: "reservations",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalState",
                schema: "booking",
                table: "reservations");
        }
    }
}
