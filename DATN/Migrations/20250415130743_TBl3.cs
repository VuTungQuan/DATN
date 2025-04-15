using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    public partial class TBl3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pitches_PitchTypes_PitchTypeID",
                table: "Pitches");

            migrationBuilder.AlterColumn<int>(
                name: "PitchTypeID",
                table: "Pitches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pitches_PitchTypes_PitchTypeID",
                table: "Pitches",
                column: "PitchTypeID",
                principalTable: "PitchTypes",
                principalColumn: "PitchTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pitches_PitchTypes_PitchTypeID",
                table: "Pitches");

            migrationBuilder.AlterColumn<int>(
                name: "PitchTypeID",
                table: "Pitches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pitches_PitchTypes_PitchTypeID",
                table: "Pitches",
                column: "PitchTypeID",
                principalTable: "PitchTypes",
                principalColumn: "PitchTypeID");
        }
    }
}
