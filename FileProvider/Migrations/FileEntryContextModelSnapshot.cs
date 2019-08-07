﻿// <auto-generated />
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WorkNet.FileProvider.Models;

namespace WorkNet.FileProvider.Migrations
{
    [DbContext(typeof(FileEntryContext))]
    partial class FileEntryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("WorkNet.FileProvider.Models.FileEntry", b =>
                {
                    b.Property<int>("FileEntryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("file_entry_id");

                    b.Property<string>("ETag")
                        .HasColumnName("etag");

                    b.Property<string>("ExtName")
                        .HasColumnName("ext_name");

                    b.Property<string>("FileName")
                        .HasColumnName("file_name");

                    b.Property<string>("Metadata")
                        .HasColumnName("metadata")
                        .HasColumnType("jsonb");

                    b.Property<string>("Namespace")
                        .HasColumnName("namespace");

                    b.Property<string>("SeaweedId")
                        .HasColumnName("seaweed_id");

                    b.Property<int>("Size")
                        .HasColumnName("size");

                    b.Property<List<string>>("Tags")
                        .HasColumnName("tags");

                    b.HasKey("FileEntryID")
                        .HasName("pk_file_entries");

                    b.ToTable("file_entries");
                });
#pragma warning restore 612, 618
        }
    }
}
