using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeRental_System3.Migrations
{
    /// <inheritdoc />
    public partial class Inventoryrejected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalRecords_Inventories_RegistrationNumber",
                table: "RentalRecords");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_RentalRecords_RegistrationNumber",
                table: "RentalRecords");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "RentalRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "RentalRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalRecords_UnitId",
                table: "RentalRecords",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalRecords_BikeUnits_UnitId",
                table: "RentalRecords",
                column: "UnitId",
                principalTable: "BikeUnits",
                principalColumn: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalRecords_BikeUnits_UnitId",
                table: "RentalRecords");

            migrationBuilder.DropIndex(
                name: "IX_RentalRecords_UnitId",
                table: "RentalRecords");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "RentalRecords");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "RentalRecords",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    RegistrationNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BikeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    YearofManufacture = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.RegistrationNumber);
                    table.ForeignKey(
                        name: "FK_Inventories_Bikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Bikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalRecords_RegistrationNumber",
                table: "RentalRecords",
                column: "RegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_BikeId",
                table: "Inventories",
                column: "BikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalRecords_Inventories_RegistrationNumber",
                table: "RentalRecords",
                column: "RegistrationNumber",
                principalTable: "Inventories",
                principalColumn: "RegistrationNumber");
        }
    }
}
