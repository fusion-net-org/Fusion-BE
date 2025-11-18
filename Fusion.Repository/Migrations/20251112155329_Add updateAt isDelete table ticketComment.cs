using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddupdateAtisDeletetableticketComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "visibility",
                table: "TicketComments");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "TicketComments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_at",
                table: "TicketComments",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "update_at",
                table: "TicketComments");

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "TicketComments",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);
        }
    }
}
