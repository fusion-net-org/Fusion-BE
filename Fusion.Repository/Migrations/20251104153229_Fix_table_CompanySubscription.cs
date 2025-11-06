using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Fix_table_CompanySubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySubscriptionAssignments_User",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.RenameColumn(
                name: "CompanyMemberId",
                table: "CompanySubscriptionAssignments",
                newName: "company_member_id");

            migrationBuilder.RenameColumn(
                name: "member_id",
                table: "CompanySubscriptionAssignments",
                newName: "user_subscription_id");

            migrationBuilder.RenameIndex(
                name: "IX_CompanySubscriptionAssignments_member_id",
                table: "CompanySubscriptionAssignments",
                newName: "IX_CompanySubscriptionAssignments_user_subscription_id");

            migrationBuilder.AlterColumn<string>(
                name: "code_transaction",
                table: "CompanySubscriptionAssignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<long>(
                name: "company_member_id",
                table: "CompanySubscriptionAssignments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CompanyMemberId1",
                table: "CompanySubscriptionAssignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId1",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId1");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments",
                columns: new[] { "company_member_id", "code_transaction" },
                unique: true,
                filter: "([company_member_id] IS NOT NULL AND [code_transaction] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMember",
                table: "CompanySubscriptionAssignments",
                column: "company_member_id",
                principalTable: "CompanyMembers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId1",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId1",
                principalTable: "CompanyMembers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySubscriptionAssignments_UserSubscription",
                table: "CompanySubscriptionAssignments",
                column: "user_subscription_id",
                principalTable: "UserSubscriptions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMember",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId1",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySubscriptionAssignments_UserSubscription",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId1",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.DropColumn(
                name: "CompanyMemberId1",
                table: "CompanySubscriptionAssignments");

            migrationBuilder.RenameColumn(
                name: "company_member_id",
                table: "CompanySubscriptionAssignments",
                newName: "CompanyMemberId");

            migrationBuilder.RenameColumn(
                name: "user_subscription_id",
                table: "CompanySubscriptionAssignments",
                newName: "member_id");

            migrationBuilder.RenameIndex(
                name: "IX_CompanySubscriptionAssignments_user_subscription_id",
                table: "CompanySubscriptionAssignments",
                newName: "IX_CompanySubscriptionAssignments_member_id");

            migrationBuilder.AlterColumn<string>(
                name: "code_transaction",
                table: "CompanySubscriptionAssignments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompanyMemberId",
                table: "CompanySubscriptionAssignments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                table: "CompanySubscriptionAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubscriptionAssignments_CompanyMemberId",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId");

            migrationBuilder.CreateIndex(
                name: "UX_CompanySubscriptionAssignments_Unique",
                table: "CompanySubscriptionAssignments",
                columns: new[] { "company_id", "member_id", "code_transaction" },
                unique: true,
                filter: "([company_id] IS NOT NULL AND [member_id] IS NOT NULL AND [code_transaction] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySubscriptionAssignments_CompanyMembers_CompanyMemberId",
                table: "CompanySubscriptionAssignments",
                column: "CompanyMemberId",
                principalTable: "CompanyMembers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySubscriptionAssignments_User",
                table: "CompanySubscriptionAssignments",
                column: "member_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
