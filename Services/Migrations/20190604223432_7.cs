using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolBuses_Devices_DeviceId",
                table: "SchoolBuses");

            migrationBuilder.DropIndex(
                name: "IX_SchoolBuses_DeviceId",
                table: "SchoolBuses");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "SchoolBuses",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_DeviceId",
                table: "SchoolBuses",
                column: "DeviceId",
                unique: true,
                filter: "[DeviceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolBuses_Devices_DeviceId",
                table: "SchoolBuses",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolBuses_Devices_DeviceId",
                table: "SchoolBuses");

            migrationBuilder.DropIndex(
                name: "IX_SchoolBuses_DeviceId",
                table: "SchoolBuses");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "SchoolBuses",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_DeviceId",
                table: "SchoolBuses",
                column: "DeviceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolBuses_Devices_DeviceId",
                table: "SchoolBuses",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
