using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRecycle.Migrations
{
    /// <inheritdoc />
    public partial class after_add_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceLogs_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 23, 41, 24, 846, DateTimeKind.Local).AddTicks(8954));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 23, 41, 24, 846, DateTimeKind.Local).AddTicks(8960));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 23, 41, 24, 846, DateTimeKind.Local).AddTicks(8964));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 23, 41, 24, 846, DateTimeKind.Local).AddTicks(8967));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 23, 41, 24, 846, DateTimeKind.Local).AddTicks(8971));

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_MachineId",
                table: "MaintenanceLogs",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceLogs");

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 18, 26, 509, DateTimeKind.Local).AddTicks(6511));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 18, 26, 509, DateTimeKind.Local).AddTicks(6517));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 18, 26, 509, DateTimeKind.Local).AddTicks(6522));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 18, 26, 509, DateTimeKind.Local).AddTicks(6526));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 18, 26, 509, DateTimeKind.Local).AddTicks(6531));
        }
    }
}
