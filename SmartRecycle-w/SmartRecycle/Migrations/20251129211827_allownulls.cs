using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRecycle.Migrations
{
    /// <inheritdoc />
    public partial class allownulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reply",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RepliedBy",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reply",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RepliedBy",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
    }
}
