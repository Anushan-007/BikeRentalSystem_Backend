using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeRental_System3.Migrations
{
    /// <inheritdoc />
    public partial class test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_BikeUnits_UnitId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UnitId",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "BikeUnitUnitId",
                table: "Images",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_BikeUnitUnitId",
                table: "Images",
                column: "BikeUnitUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BikeUnits_BikeUnitUnitId",
                table: "Images",
                column: "BikeUnitUnitId",
                principalTable: "BikeUnits",
                principalColumn: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_BikeUnits_BikeUnitUnitId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_BikeUnitUnitId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "BikeUnitUnitId",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_Images_UnitId",
                table: "Images",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BikeUnits_UnitId",
                table: "Images",
                column: "UnitId",
                principalTable: "BikeUnits",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
