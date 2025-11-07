using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectHiredField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "company_hired_id",
                table: "Projects",
                newName: "CompanyHiredId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_company_hired_id",
                table: "Projects",
                newName: "IX_Projects_CompanyHiredId");

            migrationBuilder.AddColumn<Guid>(
                name: "company_request_id",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "company_request_id",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "CompanyHiredId",
                table: "Projects",
                newName: "company_hired_id");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_CompanyHiredId",
                table: "Projects",
                newName: "IX_Projects_company_hired_id");
        }
    }
}
