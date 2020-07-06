using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "StudentCardHistoryItems",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Schools",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SchoolGradeRooms",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SchoolBuses",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PushLogs",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "NewsItems",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Devices",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Cards",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Alerts",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "StudentCardHistoryItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SchoolGradeRooms");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SchoolBuses");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PushLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Alerts");
        }
    }
}
