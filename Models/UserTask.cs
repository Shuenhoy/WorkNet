using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace WorkNet.Server.Models
{
    public class TaskContext : DbContext
    {
        public DbSet<UserTask> UserTasks { get; set; }
    }
    public class Executor
    {
        public long ExecutorId;
        public string Image;
        public string Execution;
    }
    public class UserTask
    {
        public int UserTaskId { get; set; }
        public Executor executor { get; set; }
        public int SubFinished { get; set; }
        public int SubTotal { get; set; }
        public ICollection<ExecTask> SubTasks { get; set; }

    }
    public class ExecTask
    {
        public int ExecTaskId { get; set; }

        public string[] Parameters { get; set; }
        public string[] Pulls { get; set; }
        public int Status;
        public int UserTaskId;
        public UserTask UserTask { get; set; }
    }
}