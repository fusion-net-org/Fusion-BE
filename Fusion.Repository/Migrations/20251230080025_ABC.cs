using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ABC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "update_at",
                table: "ProjectTasks",
                type: "datetime2(3)",
                precision: 3,
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "update_at",
                table: "ProjectTasks",
                type: "datetime2(3)",
                precision: 3,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldPrecision: 3,
                oldNullable: true,
                oldDefaultValueSql: "(sysutcdatetime())");
        }
    }
}
