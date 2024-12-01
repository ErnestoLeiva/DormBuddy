using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormBuddy.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalMembersToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalMembers",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMembers",
                table: "Groups");
        }
    }
}
