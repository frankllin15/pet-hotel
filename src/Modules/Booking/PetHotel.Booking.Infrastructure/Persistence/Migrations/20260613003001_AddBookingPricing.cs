using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "daily_rate",
                schema: "booking",
                table: "reservations",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                schema: "booking",
                table: "reservations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_rate",
                schema: "booking",
                table: "accommodations",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_rate",
                schema: "booking",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "total_amount",
                schema: "booking",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "daily_rate",
                schema: "booking",
                table: "accommodations");
        }
    }
}
