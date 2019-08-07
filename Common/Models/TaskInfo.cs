using System.Collections.Generic;
using System.Text.Json;
namespace WorkNet.Common.Models
{

    public class TaskSubmit
    {
        public Executor executor { get; set; }
        public List<SingleTask> tasks { get; set; }
    }
    public class GroupInfo
    {
        public long Id { get; set; }
        public string Image { get; set; }
        public string Execution { get; set; }
        public int? Executor { get; set; }
        public List<JsonElement> Parameters { get; set; }
        public List<int> Pulls { get; set; }
    }
}