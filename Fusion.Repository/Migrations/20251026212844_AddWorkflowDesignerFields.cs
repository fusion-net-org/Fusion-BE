using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowDesignerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "WorkflowTransitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role_names_json",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rule",
                table: "WorkflowTransitions",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "WorkflowStatus",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "roles_json",
                table: "WorkflowStatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "x",
                table: "WorkflowStatus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "y",
                table: "WorkflowStatus",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "label",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "role_names_json",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "rule",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "color",
                table: "WorkflowStatus");

            migrationBuilder.DropColumn(
                name: "roles_json",
                table: "WorkflowStatus");

            migrationBuilder.DropColumn(
                name: "x",
                table: "WorkflowStatus");

            migrationBuilder.DropColumn(
                name: "y",
                table: "WorkflowStatus");
        }
    }
}
