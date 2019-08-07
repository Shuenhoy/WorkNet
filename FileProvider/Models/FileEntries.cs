using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Marques.EFCore.SnakeCase;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkNet.FileProvider.Models
{
    public class FileEntryContext : DbContext
    {
        public FileEntryContext(DbContextOptions<FileEntryContext> options)
            : base(options)
        {
        }
        public FileEntryContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=database;Database=db;Username=postgre;Password=password");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // This is the only line you need to add in your context
            modelBuilder.ToSnakeCase();
        }
        public DbSet<FileEntry> FileEntries { get; set; }
    }
    public class ReadonlyFileEntryContext : DbContext
    {
        public ReadonlyFileEntryContext(DbContextOptions<ReadonlyFileEntryContext> options)
            : base(options)
        {
        }
        public ReadonlyFileEntryContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=database;Database=db;Username=read_only_user;Password=123456");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // This is the only line you need to add in your context
            modelBuilder.ToSnakeCase();
        }
        public DbSet<FileEntry> FileEntries { get; set; }
    }
    public class FileEntry
    {
        public int FileEntryID { get; set; }
        public string SeaweedId { get; set; }
        [Column(TypeName = "jsonb")]
        [JsonIgnore]
        [JsonPropertyName("__metaraw")]
        public string Metadata { get; set; }
        [NotMapped]
        [JsonPropertyName("Metadata")]
        public JsonElement _Metadata
        {
            get => JsonDocument.Parse(Metadata is null ? "{}" : Metadata).RootElement;
            set => Metadata = value.ToString();
        }

        public int Size { get; set; }
        public string ETag { get; set; }
        public List<string> Tags { get; set; }
        public string ExtName { get; set; }
        public string FileName { get; set; }
        public string Namespace { get; set; }

    }
}