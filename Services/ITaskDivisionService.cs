using System;
using System.Collections.Generic;
using WorkNet.Server.Models;

namespace WorkNet.Server.Services
{
    public interface ITaskDivisionService
    {
        IEnumerable<ExecTask> Divide(UserTask input);
    }
}