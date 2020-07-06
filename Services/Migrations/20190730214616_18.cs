using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Patent",
                table: "SchoolBuses",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Patent",
                table: "SchoolBuses");
        }
    }
}
