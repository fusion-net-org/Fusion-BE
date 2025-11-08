using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Addreasondeleteforprojectrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "reason",
                table: "ProjectRequests",
                newName: "reason_reject");

            migrationBuilder.AddColumn<string>(
                name: "reason_delete",
                table: "ProjectRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reason_delete",
                table: "ProjectRequests");

            migrationBuilder.RenameColumn(
                name: "reason_reject",
                table: "ProjectRequests",
                newName: "reason");
        }
    }
}
