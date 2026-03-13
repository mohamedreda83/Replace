using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRecycle.Migrations
{
    /// <inheritdoc />
    public partial class removeuserid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ContactMessages");

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 10, 38, 504, DateTimeKind.Local).AddTicks(5455));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 10, 38, 504, DateTimeKind.Local).AddTicks(5462));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 10, 38, 504, DateTimeKind.Local).AddTicks(5467));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 10, 38, 504, DateTimeKind.Local).AddTicks(5472));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 23, 10, 38, 504, DateTimeKind.Local).AddTicks(5477));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(229));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(234));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "Rules",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(245));
        }
    }
}
