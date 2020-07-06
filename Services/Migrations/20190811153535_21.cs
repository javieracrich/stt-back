using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations
{
    public partial class _21 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SchoolGradeRooms_GradeRoomId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SchoolBuses_SchoolBusId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SchoolBuses");

            migrationBuilder.DropTable(
                name: "SchoolGradeRooms");

            migrationBuilder.RenameColumn(
                name: "SchoolBusId",
                table: "AspNetUsers",
                newName: "RoomId");

            migrationBuilder.RenameColumn(
                name: "GradeRoomId",
                table: "AspNetUsers",
                newName: "BusId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_SchoolBusId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_GradeRoomId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_BusId");

            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Code = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    Patent = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    SchoolId = table.Column<int>(nullable: true),
                    DeviceId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buses_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    Code = table.Column<Guid>(nullable: false),
                    SchoolId = table.Column<int>(nullable: true),
                    Grade = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buses_Code",
                table: "Buses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buses_DeviceId",
                table: "Buses",
                column: "DeviceId",
                unique: true,
                filter: "[DeviceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_Patent",
                table: "Buses",
                column: "Patent",
                unique: true,
                filter: "[Patent] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_SchoolId",
                table: "Buses",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Code",
                table: "Rooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_SchoolId",
                table: "Rooms",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Buses_BusId",
                table: "AspNetUsers",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Rooms_RoomId",
                table: "AspNetUsers",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Buses_BusId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Rooms_RoomId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "AspNetUsers",
                newName: "SchoolBusId");

            migrationBuilder.RenameColumn(
                name: "BusId",
                table: "AspNetUsers",
                newName: "GradeRoomId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_RoomId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_SchoolBusId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_BusId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_GradeRoomId");

            migrationBuilder.CreateTable(
                name: "SchoolBuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    DeviceId = table.Column<int>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    Patent = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    SchoolId = table.Column<int>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolBuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolBuses_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolBuses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolGradeRooms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<Guid>(nullable: true),
                    Grade = table.Column<int>(nullable: false),
                    IsDisabled = table.Column<bool>(nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    SchoolId = table.Column<int>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true)
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
                name: "IX_SchoolBuses_Code",
                table: "SchoolBuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_DeviceId",
                table: "SchoolBuses",
                column: "DeviceId",
                unique: true,
                filter: "[DeviceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_Patent",
                table: "SchoolBuses",
                column: "Patent",
                unique: true,
                filter: "[Patent] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBuses_SchoolId",
                table: "SchoolBuses",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolGradeRooms_Code",
                table: "SchoolGradeRooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolGradeRooms_SchoolId",
                table: "SchoolGradeRooms",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SchoolGradeRooms_GradeRoomId",
                table: "AspNetUsers",
                column: "GradeRoomId",
                principalTable: "SchoolGradeRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SchoolBuses_SchoolBusId",
                table: "AspNetUsers",
                column: "SchoolBusId",
                principalTable: "SchoolBuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
