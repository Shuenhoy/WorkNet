using System.Collections.Generic;

using System;
using MessagePack;

namespace WorkNet.Common.Models
{

    [MessagePackObject(keyAsPropertyName: true)]
    public class AtomicTask
    {
        public long id;
        public IDictionary<string, object> parameters;

    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class Executor
    {
        public FileGetter worker;
        public string source;
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class AtomicTaskResult
    {
        public long groupId;
        public long atomicId;
        public Dictionary<string, object> ret;
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class TaskGroup
    {
        public List<AtomicTask> subtasks;
        public List<(string fileName, FileGetter file)> files;
        public Executor executor;
        public IDictionary<string, object> parameters;

    }
}