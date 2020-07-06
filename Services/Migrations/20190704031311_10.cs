using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType",
                table: "PushLogs");

            migrationBuilder.CreateIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType_SchoolCode",
                table: "PushLogs",
                columns: new[] { "DeviceCode", "CardCode", "PushType", "SchoolCode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType_SchoolCode",
                table: "PushLogs");

            migrationBuilder.CreateIndex(
                name: "IX_PushLogs_DeviceCode_CardCode_PushType",
                table: "PushLogs",
                columns: new[] { "DeviceCode", "CardCode", "PushType" },
                unique: true);
        }
    }
}
