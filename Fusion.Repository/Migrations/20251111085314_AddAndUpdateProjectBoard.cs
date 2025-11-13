using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddAndUpdateProjectBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "WorkflowStatus",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "WorkflowStatus",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "TaskWorkflow",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "capacity_hours",
                table: "Sprints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "committed_points",
                table: "Sprints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "carry_over_count",
                table: "ProjectTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "ProjectTasks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "current_status_id",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "estimate_hours",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_task_id",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "remaining_hours",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "severity",
                table: "ProjectTasks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "source_task_id",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_current_status_id",
                table: "ProjectTasks",
                column: "current_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_parent_task_id",
                table: "ProjectTasks",
                column: "parent_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_source_task_id",
                table: "ProjectTasks",
                column: "source_task_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_parent_task_id",
                table: "ProjectTasks",
                column: "parent_task_id",
                principalTable: "ProjectTasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_source_task_id",
                table: "ProjectTasks",
                column: "source_task_id",
                principalTable: "ProjectTasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_WorkflowStatus_current_status_id",
                table: "ProjectTasks",
                column: "current_status_id",
                principalTable: "WorkflowStatus",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_parent_task_id",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_source_task_id",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_WorkflowStatus_current_status_id",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_current_status_id",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_parent_task_id",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_source_task_id",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "category",
                table: "WorkflowStatus");

            migrationBuilder.DropColumn(
                name: "code",
                table: "WorkflowStatus");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "TaskWorkflow");

            migrationBuilder.DropColumn(
                name: "capacity_hours",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "committed_points",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "carry_over_count",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "code",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "current_status_id",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "estimate_hours",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "parent_task_id",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "remaining_hours",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "severity",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "source_task_id",
                table: "ProjectTasks");
        }
    }
}
