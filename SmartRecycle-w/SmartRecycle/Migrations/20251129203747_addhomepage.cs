using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartRecycle.Migrations
{
    /// <inheritdoc />
    public partial class addhomepage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsReplied = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepliedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatingValue = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Rules",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Order", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(229), "يجب تسجيل الدخول عبر مسح رمز QR الموجود على الماكينة قبل البدء باستخدامها", true, 1, "تسجيل الدخول مطلوب" },
                    { 2, new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(234), "مدة الجلسة 5 دقائق فقط، وسيتم تسجيل الخروج تلقائياً بعد انتهاء المدة", true, 2, "وقت الجلسة محدود" },
                    { 3, new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(238), "يجب الضغط على زر الاكتشاف مع كل عملية لضمان وجودك أمام الماكينة وحماية حسابك", true, 3, "اضغط للاكتشاف" },
                    { 4, new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(242), "تأكد من أن المواد المعاد تدويرها نظيفة وخالية من السوائل والأوساخ", true, 4, "مواد نظيفة فقط" },
                    { 5, new DateTime(2025, 11, 29, 22, 37, 45, 162, DateTimeKind.Local).AddTicks(245), "يجب تسجيل الخروج بعد الانتهاء لحماية حسابك ونقاطك من الاستخدام غير المصرح به", true, 5, "لا تترك حسابك مفتوحاً" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Rules");
        }
    }
}
