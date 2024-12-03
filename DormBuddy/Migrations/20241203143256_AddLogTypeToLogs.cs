using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormBuddy.Migrations
{
    public partial class AddLogTypeToLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<string>(
                name: "LogType",
                table: "Logs",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "LogType",
                table: "Logs");
        }
    }
}
