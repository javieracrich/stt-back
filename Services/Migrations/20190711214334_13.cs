using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SchoolGradeRooms");

            migrationBuilder.AddColumn<Guid>(
                name: "Code",
                table: "NewsItems",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "NewsItems");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SchoolGradeRooms",
                unicode: false,
                maxLength: 500,
                nullable: true);
        }
    }
}
