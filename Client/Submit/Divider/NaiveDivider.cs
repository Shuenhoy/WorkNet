using System.Collections.Generic;
using WorkNet.Common.Models;
using System;
using System.Linq;
using LanguageExt;

namespace WorkNet.Client.Submit.Divider
{
    class NaiveDivider : ITaskDivider
    {
        public int num;

        public IEnumerable<TaskGroup> Divide(RawTaskGroup group)
        {
            int count = Math.Max(num, 5);
            return group.subtasks
                .Select((x, i) => (x, i))
                .GroupBy(x => x.i % count)
                .Select(x => new TaskGroup()
                {
                    executor = group.executor,
                    parameters = group.parameters,
                    subtasks = x.Map(y => new AtomicTask()
                    {
                        id = y.i,
                        parameters = y.x.parameters
                    }).ToList(),
                    files = x.Map(y => y.x.files)
                        .Flatten()
                        .DistinctBy(y => y.Item1).ToList()
                });
        }
    }
}