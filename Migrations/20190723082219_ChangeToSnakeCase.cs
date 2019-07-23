using Microsoft.EntityFrameworkCore.Migrations;

namespace WorkNet.FileProvider.Migrations
{
    public partial class ChangeToSnakeCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FileEntries",
                table: "FileEntries");

            migrationBuilder.RenameTable(
                name: "FileEntries",
                newName: "file_entries");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "file_entries",
                newName: "tags");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "file_entries",
                newName: "size");

            migrationBuilder.RenameColumn(
                name: "Namespace",
                table: "file_entries",
                newName: "namespace");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "file_entries",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "ETag",
                table: "file_entries",
                newName: "etag");

            migrationBuilder.RenameColumn(
                name: "SeaweedId",
                table: "file_entries",
                newName: "seaweed_id");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "file_entries",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "ExtName",
                table: "file_entries",
                newName: "ext_name");

            migrationBuilder.RenameColumn(
                name: "FileEntryID",
                table: "file_entries",
                newName: "file_entry_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_file_entries",
                table: "file_entries",
                column: "file_entry_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_file_entries",
                table: "file_entries");

            migrationBuilder.RenameTable(
                name: "file_entries",
                newName: "FileEntries");

            migrationBuilder.RenameColumn(
                name: "tags",
                table: "FileEntries",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "size",
                table: "FileEntries",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "namespace",
                table: "FileEntries",
                newName: "Namespace");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "FileEntries",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "etag",
                table: "FileEntries",
                newName: "ETag");

            migrationBuilder.RenameColumn(
                name: "seaweed_id",
                table: "FileEntries",
                newName: "SeaweedId");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "FileEntries",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "ext_name",
                table: "FileEntries",
                newName: "ExtName");

            migrationBuilder.RenameColumn(
                name: "file_entry_id",
                table: "FileEntries",
                newName: "FileEntryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileEntries",
                table: "FileEntries",
                column: "FileEntryID");
        }
    }
}
