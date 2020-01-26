using System.Collections.Generic;

using System;
using MessagePack;

namespace WorkNet.Common.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class TaskFail
    {
        public long id;
        public string message;
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class TaskResult
    {
        public long id;
        public Dictionary<string, object> payload;
    }
}