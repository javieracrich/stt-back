using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "Cards",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_SchoolId",
                table: "Cards",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Schools_SchoolId",
                table: "Cards",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Schools_SchoolId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_SchoolId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Cards");
        }
    }
}
