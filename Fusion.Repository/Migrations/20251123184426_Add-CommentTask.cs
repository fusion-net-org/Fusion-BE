using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "comment_id",
                table: "project_task_attachments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_task_attachments_comment_id",
                table: "project_task_attachments",
                column: "comment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_project_task_attachments_Comments_comment_id",
                table: "project_task_attachments",
                column: "comment_id",
                principalTable: "Comments",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_task_attachments_Comments_comment_id",
                table: "project_task_attachments");

            migrationBuilder.DropIndex(
                name: "IX_project_task_attachments_comment_id",
                table: "project_task_attachments");

            migrationBuilder.DropColumn(
                name: "comment_id",
                table: "project_task_attachments");
        }
    }
}
