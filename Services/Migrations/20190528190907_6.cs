using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SchoolCode",
                table: "Schools",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Schools_SchoolCode",
                table: "Schools",
                newName: "IX_Schools_Code");

            migrationBuilder.RenameColumn(
                name: "UserCode",
                table: "AspNetUsers",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_UserCode",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Schools",
                newName: "SchoolCode");

            migrationBuilder.RenameIndex(
                name: "IX_Schools_Code",
                table: "Schools",
                newName: "IX_Schools_SchoolCode");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "AspNetUsers",
                newName: "UserCode");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_UserCode");
        }
    }
}
