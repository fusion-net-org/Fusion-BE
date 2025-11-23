using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTaskAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_task_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    task_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    public_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_image = table.Column<bool>(type: "bit", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_task_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_task_attachments_ProjectTasks_task_id",
                        column: x => x.task_id,
                        principalTable: "ProjectTasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_task_attachments_task_id",
                table: "project_task_attachments",
                column: "task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_task_attachments");
        }
    }
}
