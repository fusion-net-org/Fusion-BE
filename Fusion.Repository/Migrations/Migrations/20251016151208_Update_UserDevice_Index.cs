using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserDevice_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_device_token",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_user_id",
                table: "UserDevices");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_user_id_device_token",
                table: "UserDevices",
                columns: new[] { "user_id", "device_token" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_user_id_device_token",
                table: "UserDevices");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_device_token",
                table: "UserDevices",
                column: "device_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_user_id",
                table: "UserDevices",
                column: "user_id");
        }
    }
}
