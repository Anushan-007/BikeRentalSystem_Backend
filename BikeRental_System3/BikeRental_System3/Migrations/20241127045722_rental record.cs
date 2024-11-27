using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeRental_System3.Migrations
{
    /// <inheritdoc />
    public partial class rentalrecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BikeRegNo",
                table: "RentalRecords",
                newName: "RegistrationNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "RentalRecords",
                newName: "BikeRegNo");
        }
    }
}
