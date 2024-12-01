using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormBuddy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToProfilePosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLastUpdate_AspNetUsers_UserId",
                table: "UserLastUpdate");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserLastUpdate",
                type: "varchar(160)",
                maxLength: 160,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(160)",
                oldMaxLength: 160)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TargetId",
                table: "Profile_Posts",
                type: "varchar(160)",
                maxLength: 160,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLastUpdate_AspNetUsers_UserId",
                table: "UserLastUpdate",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLastUpdate_AspNetUsers_UserId",
                table: "UserLastUpdate");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Profile_Posts");

            migrationBuilder.UpdateData(
                table: "UserLastUpdate",
                keyColumn: "UserId",
                keyValue: null,
                column: "UserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserLastUpdate",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(160)",
                oldMaxLength: 160,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLastUpdate_AspNetUsers_UserId",
                table: "UserLastUpdate",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
