using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WorkNet.Server.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Executors",
                columns: table => new
                {
                    ExecutorId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Image = table.Column<string>(nullable: true),
                    Execution = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executors", x => x.ExecutorId);
                });

            migrationBuilder.CreateTable(
                name: "UserTasks",
                columns: table => new
                {
                    UserTaskId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Image = table.Column<string>(nullable: true),
                    Execution = table.Column<string>(nullable: true),
                    SubFinished = table.Column<int>(nullable: false),
                    SubTotal = table.Column<int>(nullable: false),
                    SubmitTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTasks", x => x.UserTaskId);
                });

            migrationBuilder.CreateTable(
                name: "TaskGroups",
                columns: table => new
                {
                    TaskGroupId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserTaskId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskGroups", x => x.TaskGroupId);
                    table.ForeignKey(
                        name: "FK_TaskGroups_UserTasks_UserTaskId1",
                        column: x => x.UserTaskId1,
                        principalTable: "UserTasks",
                        principalColumn: "UserTaskId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SingleTask",
                columns: table => new
                {
                    SingleTaskId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Parameters = table.Column<string>(type: "json", nullable: true),
                    Pulls = table.Column<int[]>(nullable: true),
                    TaskGroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleTask", x => x.SingleTaskId);
                    table.ForeignKey(
                        name: "FK_SingleTask_TaskGroups_TaskGroupId",
                        column: x => x.TaskGroupId,
                        principalTable: "TaskGroups",
                        principalColumn: "TaskGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SingleTask_TaskGroupId",
                table: "SingleTask",
                column: "TaskGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskGroups_UserTaskId1",
                table: "TaskGroups",
                column: "UserTaskId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Executors");

            migrationBuilder.DropTable(
                name: "SingleTask");

            migrationBuilder.DropTable(
                name: "TaskGroups");

            migrationBuilder.DropTable(
                name: "UserTasks");
        }
    }
}
