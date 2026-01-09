using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ComponentField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "component_id",
                table: "Tickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "component_id",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_component_id",
                table: "Tickets",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_component_id",
                table: "ProjectTasks",
                column: "component_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectComponents_component_id",
                table: "ProjectTasks",
                column: "component_id",
                principalTable: "ProjectComponents",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ProjectComponents_component_id",
                table: "Tickets",
                column: "component_id",
                principalTable: "ProjectComponents",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectComponents_component_id",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ProjectComponents_component_id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_component_id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_component_id",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "component_id",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "component_id",
                table: "ProjectTasks");
        }
    }
}
