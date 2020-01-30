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
        public Dictionary<string, PayloadItem> payload;
    }
    [MessagePack.Union(0, typeof(ObjectPayload))]
    [MessagePack.Union(1, typeof(FilePair))]
    public interface PayloadItem { }
    [MessagePackObject(keyAsPropertyName: true)]

    public class ObjectPayload : PayloadItem
    {
        public object obj;
    }
    [MessagePackObject(keyAsPropertyName: true)]

    public class FilePair : PayloadItem
    {
        public string filename;
        public FileGetter file;

    }
}