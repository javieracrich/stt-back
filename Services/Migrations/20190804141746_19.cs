using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_Patent",
                table: "SchoolBuses",
                column: "Patent",
                unique: true,
                filter: "[Patent] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolBuses_Patent",
                table: "SchoolBuses");
        }
    }
}
