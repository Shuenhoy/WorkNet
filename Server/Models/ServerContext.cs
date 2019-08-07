using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WorkNet.Common.Models;


namespace WorkNet.Server.Models
{
    public class ServerContext : DbContext
    {
        public ServerContext() { }
        public ServerContext(DbContextOptions<ServerContext> options)
          : base(options)
        {
            Agents = new HashSet<AgentInfo>();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseNpgsql("Host=database;Database=db;Username=postgre;Password=password");
        }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<TaskGroup> TaskGroups { get; set; }
        public DbSet<Executor> Executors { get; set; }
        public HashSet<AgentInfo> Agents { get; set; }
    }
}