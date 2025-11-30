using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketid_ProjectTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ticket_id",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ticket_id",
                table: "ProjectTasks",
                column: "ticket_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_Tickets_ticket_id",
                table: "ProjectTasks",
                column: "ticket_id",
                principalTable: "Tickets",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_Tickets_ticket_id",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ticket_id",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "ticket_id",
                table: "ProjectTasks");
        }
    }
}
