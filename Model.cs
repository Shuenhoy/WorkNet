using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
namespace WorkNet.Client
{
    public class FileEntry
    {
        public int FileEntryID { get; set; }
        public string SeaweedId { get; set; }

        public JsonElement Metadata;

        public int Size { get; set; }
        public string ETag { get; set; }
        public List<string> Tags { get; set; }
        public string ExtName { get; set; }
        public string FileName { get; set; }
        public string Namespace { get; set; }

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

        public JsonElement Parameters { get; set; }
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