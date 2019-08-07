using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Marques.EFCore.SnakeCase;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkNet.Common.Models;

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

}