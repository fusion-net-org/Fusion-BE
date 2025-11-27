using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultCompanyMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DECLARE @constraint NVARCHAR(200);
        SELECT @constraint = dc.name
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        WHERE t.name = 'CompanyMembers' AND c.name = 'joined_at';

        IF @constraint IS NOT NULL
            EXEC('ALTER TABLE CompanyMembers DROP CONSTRAINT ' + @constraint);
    ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "joined_at",
                table: "CompanyMembers",
                type: "datetime2(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "joined_at",
                table: "CompanyMembers",
                type: "datetime2(3)",
                nullable: true,
                defaultValueSql: "(sysutcdatetime())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)",
                oldNullable: true);
        }

    }
}
