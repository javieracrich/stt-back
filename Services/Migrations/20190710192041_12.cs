using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCardHistoryItem",
                table: "StudentCardHistoryItem");

            migrationBuilder.RenameTable(
                name: "StudentCardHistoryItem",
                newName: "StudentCardHistoryItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCardHistoryItems",
                table: "StudentCardHistoryItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SchoolGradeRooms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    Code = table.Column<Guid>(nullable: false),
                    SchoolId = table.Column<int>(nullable: true),
                    Description = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    SchoolGrade = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolGradeRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolGradeRooms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolGradeRooms_Code",
                table: "SchoolGradeRooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolGradeRooms_SchoolId",
                table: "SchoolGradeRooms",
                column: "SchoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolGradeRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCardHistoryItems",
                table: "StudentCardHistoryItems");

            migrationBuilder.RenameTable(
                name: "StudentCardHistoryItems",
                newName: "StudentCardHistoryItem");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCardHistoryItem",
                table: "StudentCardHistoryItem",
                column: "Id");
        }
    }
}
