using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormBuddy.Migrations
{
    public partial class AddLogTypeAndDescriptionToLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commented out to avoid adding the LogType column if it already exists
            // migrationBuilder.AddColumn<string>(
            //     name: "LogType",
            //     table: "Logs",
            //     type: "varchar(50)",
            //     nullable: false,
            //     defaultValue: "Info");

            // Add the 'Description' column to the 'Logs' table
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Logs",
                type: "varchar(500)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the 'Description' column from the 'Logs' table
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Logs");

            // Commented out to avoid removing the LogType column
            // migrationBuilder.DropColumn(
            //     name: "LogType",
            //     table: "Logs");
        }
    }
}
