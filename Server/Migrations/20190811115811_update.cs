using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkNet.Server.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "TaskGroups",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OpExecutor",
                table: "Executors",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "TaskGroups");

            migrationBuilder.DropColumn(
                name: "OpExecutor",
                table: "Executors");
        }
    }
}
