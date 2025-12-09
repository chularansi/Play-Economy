using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Play.User.Service.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firstname = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: true),
                    lastname = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: true),
                    gil = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MessageIds",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageIds", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_MessageIds_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageIds_UserId",
                table: "MessageIds",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageIds");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
