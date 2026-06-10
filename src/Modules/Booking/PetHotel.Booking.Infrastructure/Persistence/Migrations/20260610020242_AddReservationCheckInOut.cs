using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationCheckInOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "checked_in_at",
                schema: "booking",
                table: "reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "checked_out_at",
                schema: "booking",
                table: "reservations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checked_in_at",
                schema: "booking",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "checked_out_at",
                schema: "booking",
                table: "reservations");
        }
    }
}
