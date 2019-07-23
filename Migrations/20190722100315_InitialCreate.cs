using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WorkNet.FileProvider.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileEntries",
                columns: table => new
                {
                    FileEntryID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SeaweedId = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    Size = table.Column<int>(nullable: false),
                    ETag = table.Column<string>(nullable: true),
                    Tags = table.Column<List<string>>(nullable: true),
                    ExtName = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Namespace = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileEntries", x => x.FileEntryID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileEntries");
        }
    }
}
