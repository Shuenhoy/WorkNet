using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
namespace WorkNet.Server.Models
{
    public class ServerContext : DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options)
          : base(options)
        {
            Agents = new HashSet<AgentInfo>();
        }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Executor> Executors { get; set; }
        public HashSet<AgentInfo> Agents { get; set; }
    }
}