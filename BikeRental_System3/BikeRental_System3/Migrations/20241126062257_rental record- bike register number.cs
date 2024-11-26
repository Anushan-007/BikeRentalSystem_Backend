using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeRental_System3.Migrations
{
    /// <inheritdoc />
    public partial class rentalrecordbikeregisternumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BikeRegNo",
                table: "RentalRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BikeRegNo",
                table: "RentalRecords");
        }
    }
}
