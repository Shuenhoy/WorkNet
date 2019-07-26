using System.Collections.Generic;
using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
namespace WorkNet.Server.Models
{

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
        public DateTime SubmitTime { get; set; }
        public ICollection<TaskGroup> SubTasks { get; set; }

    }
    public class SingleTask
    {
        public long SingleTaskId { get; set; }

        [Column(TypeName = "json")]
        [JsonIgnore]
        [JsonPropertyName("__pararaw")]
        public string Parameters { get; set; }
        [NotMapped]
        [JsonPropertyName("Parameters")]
        public JsonElement _Parameters
        {
            get => JsonDocument.Parse(Parameters is null ? "{}" : Parameters).RootElement;
            set => Parameters = value.ToString();
        }

        public string[] Pulls { get; set; }
    }
    public class TaskGroup
    {
        public int TaskGroupId { get; set; }
        public ICollection<SingleTask> SingleTasks { get; set; }

        public int Status;
        public int UserTaskId;
        public UserTask UserTask { get; set; }
    }
}