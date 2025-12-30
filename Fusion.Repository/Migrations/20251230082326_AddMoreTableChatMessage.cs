using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreTableChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatConversation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectPairKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatConversation_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserFriendship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PairKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddresseeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriendship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFriendship_Users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UserFriendship_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChatConversationMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversationMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatConversationMember_ChatConversation_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatConversation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatConversationMember_Users_AddedBy",
                        column: x => x.AddedBy,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ChatConversationMember_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_ChatConversation_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatConversation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessage_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversation_CreatedBy",
                table: "ChatConversation",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversationMember_AddedBy",
                table: "ChatConversationMember",
                column: "AddedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversationMember_ConversationId",
                table: "ChatConversationMember",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversationMember_UserId",
                table: "ChatConversationMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ConversationId",
                table: "ChatMessage",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_SenderId",
                table: "ChatMessage",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendship_AddresseeId",
                table: "UserFriendship",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendship_RequesterId",
                table: "UserFriendship",
                column: "RequesterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatConversationMember");

            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropTable(
                name: "UserFriendship");

            migrationBuilder.DropTable(
                name: "ChatConversation");
        }
    }
}
