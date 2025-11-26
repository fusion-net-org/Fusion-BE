using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_ProjectTaskAssignee_ProjectTaskDependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectTaskAssignees",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskAssignees", x => new { x.task_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_ProjectTaskAssignees_ProjectTasks_task_id",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTaskAssignees_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTaskDependencies",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    depends_on_task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTaskDependencies", x => new { x.task_id, x.depends_on_task_id });
                    table.ForeignKey(
                        name: "FK_ProjectTaskDependencies_ProjectTasks_depends_on_task_id",
                        column: x => x.depends_on_task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectTaskDependencies_ProjectTasks_task_id",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskAssignees_user_id",
                table: "ProjectTaskAssignees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTaskDependencies_depends_on_task_id",
                table: "ProjectTaskDependencies",
                column: "depends_on_task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTaskAssignees");

            migrationBuilder.DropTable(
                name: "ProjectTaskDependencies");
        }
    }
}
