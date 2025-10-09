using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarCompanyinCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "Sprints",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "Sprints",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Sprints",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "Sprints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "goal",
                table: "Sprints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Sprints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "status",
                table: "Sprints",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_at",
                table: "Sprints",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "ProjectTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "order_in_sprint",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatar_company",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "goal",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "update_at",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "order_in_sprint",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "avatar_company",
                table: "Companies");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "start_date",
                table: "Sprints",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "end_date",
                table: "Sprints",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
