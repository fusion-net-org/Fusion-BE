using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStatus_AddIsDeletedIntoCompanyMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "CompanyMembers",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "True",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "CompanyMembers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "CompanyMembers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "CompanyMembers");

            migrationBuilder.DropColumn(
                name: "reason",
                table: "CompanyMembers");

            migrationBuilder.AlterColumn<bool>(
                name: "status",
                table: "CompanyMembers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "True");
        }
    }
}
