using System.Collections.Generic;

using System;
using MessagePack;

namespace WorkNet.Common.Models
{

    [MessagePackObject(keyAsPropertyName: true)]
    public class AtomicTask
    {
        public long id;
        public IDictionary<object, string> parameters;

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
        public long atomicId;
        public Dictionary<string, object> ret;
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class TaskGroup
    {
        public List<AtomicTask> subtasks;
        public List<ValueTuple<string, FileGetter>> files;
        public Executor executor;
        public IDictionary<string, string> parameters;

    }
}