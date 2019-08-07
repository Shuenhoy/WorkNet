using System;
using System.Collections.Generic;
using System.Linq;

using WorkNet.Server.Models;
using WorkNet.Common.Models;

namespace WorkNet.Server.Services
{
    public class NaiveTaskDivisionService : ITaskDivisionService
    {
        ServerContext context;
        public NaiveTaskDivisionService(ServerContext c)
        {
            context = c;
        }
        public ICollection<TaskGroup> Divide(IEnumerable<SingleTask> inputs)
        {
            int agentCount = Math.Max(context.Agents.Count, 5);
            return inputs
                .Select((x, i) => (x, i))
                .GroupBy(x => x.i % agentCount)
                .Select(x => new TaskGroup() { Status = 0, SingleTasks = x.Select(y => y.x).ToList() })
                .ToList();
        }
    }
}