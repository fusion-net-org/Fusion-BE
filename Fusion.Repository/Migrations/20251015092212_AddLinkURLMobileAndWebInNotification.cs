using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkURLMobileAndWebInNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "link_url",
                table: "Notifications",
                newName: "link_url_web");

            migrationBuilder.AddColumn<string>(
                name: "link_url_mobile",
                table: "Notifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link_url_mobile",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "link_url_web",
                table: "Notifications",
                newName: "link_url");
        }
    }
}
