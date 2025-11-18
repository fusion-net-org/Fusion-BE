using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Addfieldscreatebyatupdatebyatcontract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "create_at",
                table: "Contracts",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_at",
                table: "Contracts",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_created_by",
                table: "Contracts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_updated_by",
                table: "Contracts",
                column: "updated_by");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_created_by",
                table: "Contracts",
                column: "created_by",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_updated_by",
                table: "Contracts",
                column: "updated_by",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_created_by",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_updated_by",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_created_by",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_updated_by",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "create_at",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "update_at",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "Contracts");
        }
    }
}
