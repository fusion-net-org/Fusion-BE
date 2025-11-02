using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_table_CompanySubscriptionAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySubscriptionAssignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code_transaction = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    revoked_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    CompanyMemberId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionAssignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId",
                        column: x => x.CompanyMemberId,
                        principalTable: "CompanyMembers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionAssignments_User",
                        column: x => x.member_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_member_id",
                table: "CompanySubscriptionAssignments",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments",
                columns: new[] { "company_id", "member_id", "code_transaction" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [member_id] IS NOT NULL AND [code_transaction] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySubscriptionAssignments");
        }
    }
}
