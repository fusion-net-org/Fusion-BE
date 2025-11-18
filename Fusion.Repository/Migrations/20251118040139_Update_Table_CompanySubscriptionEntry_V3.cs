using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_CompanySubscriptionEntry_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySubscriptionEntries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_member_id = table.Column<long>(type: "bigint", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySubscriptionEntries", x => x.id);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionEntries_CompanyMembers_MemberId",
                        column: x => x.company_member_id,
                        principalTable: "CompanyMembers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySubscriptionEntries_CompanySubscriptions_SubId",
                        column: x => x.company_subscription_id,
                        principalTable: "CompanySubscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionEntries_company_member_id",
                table: "CompanySubscriptionEntries",
                column: "company_member_id");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionEntries_Sub_Member",
                table: "CompanySubscriptionEntries",
                columns: new[] { "company_subscription_id", "company_member_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySubscriptionEntries");
        }
    }
}
