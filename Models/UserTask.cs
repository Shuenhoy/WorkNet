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
        public long ExecutorId { get; set; }
        public string Image { get; set; }
        public string Execution { get; set; }
        [JsonPropertyName("Executor")]
        public int? OpExecutor { get; set; }
    }
    public class UserTask
    {
        public int UserTaskId { get; set; }
        public string Image { get; set; }
        public string Execution { get; set; }
        public int? Executor { get; set; }
        public int SubFinished { get; set; }
        public int SubTotal { get; set; }
        public DateTime SubmitTime { get; set; }
        public virtual ICollection<TaskGroup> SubTasks { get; set; }

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
        public int? Result { get; set; }

        public int[] Pulls { get; set; }
    }
    public class TaskGroup
    {
        public int TaskGroupId { get; set; }
        public virtual ICollection<SingleTask> SingleTasks { get; set; }
        public string Assignment { get; set; }
        public int Status { get; set; }
        public int UserTaskId { get; set; }
        [JsonIgnore]

        public virtual UserTask UserTask { get; set; }
    }
}