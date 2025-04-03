using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    public partial class AddDiscountTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Discounts_DiscountID",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "DiscountID",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Discounts_DiscountID",
                table: "Bookings",
                column: "DiscountID",
                principalTable: "Discounts",
                principalColumn: "DiscountID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Discounts_DiscountID",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "DiscountID",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Discounts_DiscountID",
                table: "Bookings",
                column: "DiscountID",
                principalTable: "Discounts",
                principalColumn: "DiscountID");
        }
    }
}
