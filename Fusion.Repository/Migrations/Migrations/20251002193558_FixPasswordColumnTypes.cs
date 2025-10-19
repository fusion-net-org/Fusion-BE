using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
    name: "password_hash",
    table: "Users",
    type: "varbinary(256)",
    nullable: false);

            migrationBuilder.AlterColumn<byte[]>(
                name: "password_salt",
                table: "Users",
                type: "varbinary(128)",
                nullable: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
