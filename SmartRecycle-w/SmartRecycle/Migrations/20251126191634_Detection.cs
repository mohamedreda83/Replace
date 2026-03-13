using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRecycle.Migrations
{
    /// <inheritdoc />
    public partial class Detection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Detections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GlassBottles = table.Column<int>(type: "int", nullable: false),
                    PlasticBottles = table.Column<int>(type: "int", nullable: false),
                    Cans = table.Column<int>(type: "int", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Detections_Timestamp",
                table: "Detections",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Detections");
        }
    }
}
