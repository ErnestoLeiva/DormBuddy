using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormBuddy.Migrations
{
    /// <inheritdoc />
    public partial class AdditionToDashChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "DashboardChatModel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardChatModel_UserId",
                table: "DashboardChatModel",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DashboardChatModel_AspNetUsers_UserId",
                table: "DashboardChatModel",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DashboardChatModel_AspNetUsers_UserId",
                table: "DashboardChatModel");

            migrationBuilder.DropIndex(
                name: "IX_DashboardChatModel_UserId",
                table: "DashboardChatModel");

            migrationBuilder.DropColumn(
                name: "type",
                table: "DashboardChatModel");
        }
    }
}
