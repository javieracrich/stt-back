using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchoolGrade",
                table: "SchoolGradeRooms");

            migrationBuilder.RenameColumn(
                name: "Grade",
                table: "AspNetUsers",
                newName: "GradeRoomId");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "SchoolGradeRooms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GradeRoomId",
                table: "AspNetUsers",
                column: "GradeRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SchoolGradeRooms_GradeRoomId",
                table: "AspNetUsers",
                column: "GradeRoomId",
                principalTable: "SchoolGradeRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SchoolGradeRooms_GradeRoomId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GradeRoomId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "SchoolGradeRooms");

            migrationBuilder.RenameColumn(
                name: "GradeRoomId",
                table: "AspNetUsers",
                newName: "Grade");

            migrationBuilder.AddColumn<int>(
                name: "SchoolGrade",
                table: "SchoolGradeRooms",
                nullable: false,
                defaultValue: 0);
        }
    }
}
