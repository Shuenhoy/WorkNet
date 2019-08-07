using System;
using System.Collections.Generic;
using WorkNet.Server.Models;

namespace WorkNet.Server.Services
{
    public interface ITaskDivisionService
    {
        ICollection<TaskGroup> Divide(IEnumerable<SingleTask> inputs);
    }
}