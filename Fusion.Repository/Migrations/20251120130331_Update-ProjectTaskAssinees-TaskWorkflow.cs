using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectTaskAssineesTaskWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTaskId",
                table: "TaskWorkflow",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkflow_ProjectTaskId",
                table: "TaskWorkflow",
                column: "ProjectTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskWorkflow_ProjectTasks_ProjectTaskId",
                table: "TaskWorkflow",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskWorkflow_ProjectTasks_ProjectTaskId",
                table: "TaskWorkflow");

            migrationBuilder.DropIndex(
                name: "IX_TaskWorkflow_ProjectTaskId",
                table: "TaskWorkflow");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "TaskWorkflow");
        }
    }
}
