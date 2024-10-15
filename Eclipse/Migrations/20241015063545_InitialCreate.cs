using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eclipse.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conference",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MemberCount = table.Column<int>(type: "integer", nullable: false),
                    ConferencePicture = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Pfp = table.Column<string>(type: "text", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConferenceMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConferenceMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConferenceMember_Conference_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConferenceMember_User_MemberId",
                        column: x => x.MemberId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatRoom",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId2 = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PinnedMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    PinnedMessageId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRoom_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatRoom_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoom_User_UserId2",
                        column: x => x.UserId2,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageText = table.Column<string>(type: "text", nullable: true),
                    ReactionId = table.Column<string>(type: "text", nullable: true),
                    ReplyId = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_ChatRoom_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "ChatRoom",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Message_User_SenderId",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_PinnedMessageId1",
                table: "ChatRoom",
                column: "PinnedMessageId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_UserId",
                table: "ChatRoom",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_UserId1",
                table: "ChatRoom",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_UserId2",
                table: "ChatRoom",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceMember_ConferenceId",
                table: "ConferenceMember",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceMember_MemberId",
                table: "ConferenceMember",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatRoomId",
                table: "Message",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SenderId",
                table: "Message",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Message_PinnedMessageId1",
                table: "ChatRoom",
                column: "PinnedMessageId1",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Message_PinnedMessageId1",
                table: "ChatRoom");

            migrationBuilder.DropTable(
                name: "ConferenceMember");

            migrationBuilder.DropTable(
                name: "Conference");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "ChatRoom");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
