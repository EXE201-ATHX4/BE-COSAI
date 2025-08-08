using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class modifyShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShippingMethodId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingMethodId",
                table: "Shipments",
                column: "ShippingMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShippingMethodId",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments",
                column: "ShippingAddressId",
                unique: true,
                filter: "[ShippingAddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingMethodId",
                table: "Shipments",
                column: "ShippingMethodId",
                unique: true,
                filter: "[ShippingMethodId] IS NOT NULL");
        }
    }
}
