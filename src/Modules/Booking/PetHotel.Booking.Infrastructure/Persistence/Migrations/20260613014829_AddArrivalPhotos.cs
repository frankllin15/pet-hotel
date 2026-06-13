using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHotel.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArrivalPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "ArrivalPhotoKeys",
                schema: "booking",
                table: "reservations",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'"); // linhas existentes recebem array vazio
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalPhotoKeys",
                schema: "booking",
                table: "reservations");
        }
    }
}
