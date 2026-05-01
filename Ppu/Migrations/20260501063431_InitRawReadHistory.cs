using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ppu.Migrations
{
    /// <inheritdoc />
    public partial class InitRawReadHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawReadHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimestampUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    FunctionCode = table.Column<int>(type: "INTEGER", nullable: false),
                    StartAddress = table.Column<ushort>(type: "INTEGER", nullable: false),
                    RegisterCount = table.Column<ushort>(type: "INTEGER", nullable: false),
                    RegistersJson = table.Column<string>(type: "TEXT", maxLength: 270, nullable: false),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawReadHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawReadHistory_AppRunId",
                table: "RawReadHistory",
                column: "AppRunId");

            migrationBuilder.CreateIndex(
                name: "IX_RawReadHistory_TimestampUtc",
                table: "RawReadHistory",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawReadHistory");
        }
    }
}
