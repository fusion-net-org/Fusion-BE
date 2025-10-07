using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_Status_Table_Comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Comments");
        }
    }
}
