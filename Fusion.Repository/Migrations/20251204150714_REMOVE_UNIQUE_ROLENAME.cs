using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class REMOVE_UNIQUE_ROLENAME : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Roles_Company_RoleName",
                table: "Roles");

            migrationBuilder.CreateIndex(
                name: "UX_Roles_Company_RoleName",
                table: "Roles",
                columns: new[] { "company_id", "role_name" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [role_name] IS NOT NULL AND [status] = 'Active')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Roles_Company_RoleName",
                table: "Roles");

            migrationBuilder.CreateIndex(
                name: "UX_Roles_Company_RoleName",
                table: "Roles",
                columns: new[] { "company_id", "role_name" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [role_name] IS NOT NULL)");
        }
    }
}
