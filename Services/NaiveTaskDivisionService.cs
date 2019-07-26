using System;
using System.Collections.Generic;

using WorkNet.Server.Models;
namespace WorkNet.Server.Services
{
    public class NaiveTaskDivisionService : ITaskDivisionService
    {
        ServerContext context;
        public NaiveTaskDivisionService(ServerContext c)
        {
            context = c;
        }
        public IEnumerable<ExecTask> Divide(UserTask input)
        {
            int agentCount = context.Agents.Count;
            throw new NotImplementedException();
        }
    }
}