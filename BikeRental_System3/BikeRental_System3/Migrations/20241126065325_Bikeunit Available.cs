using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeRental_System3.Migrations
{
    /// <inheritdoc />
    public partial class BikeunitAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Availability",
                table: "BikeUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BikeUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "BikeUnits");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BikeUnits");
        }
    }
}
