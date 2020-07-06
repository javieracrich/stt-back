using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PushLogs_DeviceCode_CardCode",
                table: "PushLogs");

            migrationBuilder.AddColumn<int>(
                name: "PushType",
                table: "PushLogs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType",
                table: "PushLogs",
                columns: new[] { "DeviceCode", "CardCode", "PushType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType",
                table: "PushLogs");

            migrationBuilder.DropColumn(
                name: "PushType",
                table: "PushLogs");

            migrationBuilder.CreateIndex(
                name: "IX_PushLogs_DeviceCode_CardCode",
                table: "PushLogs",
                columns: new[] { "DeviceCode", "CardCode" },
                unique: true);
        }
    }
}
