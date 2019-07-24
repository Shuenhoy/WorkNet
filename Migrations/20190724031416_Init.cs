using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WorkNet.FileProvider.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_entries",
                columns: table => new
                {
                    file_entry_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    seaweed_id = table.Column<string>(nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    size = table.Column<int>(nullable: false),
                    etag = table.Column<string>(nullable: true),
                    tags = table.Column<List<string>>(nullable: true),
                    ext_name = table.Column<string>(nullable: true),
                    file_name = table.Column<string>(nullable: true),
                    @namespace = table.Column<string>(name: "namespace", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_entries", x => x.file_entry_id);
                });
            migrationBuilder.Sql("create role read_only_user login noinherit  password '123456';");
            migrationBuilder.Sql("grant select on file_entries to read_only_user;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_entries");
        }
    }
}
