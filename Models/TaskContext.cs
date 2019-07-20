using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
namespace WorkNet.Server.Models
{
    public class TaskContext : DbContext
    {
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<Executor> Executors { get; set; }
    }
}