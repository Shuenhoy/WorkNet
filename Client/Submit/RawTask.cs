using System;
using System.Collections.Generic;
using WorkNet.Common.Models;

namespace WorkNet.Client.Submit
{
    public class RawTask<T>
    {
        public long id;
        public IDictionary<object, string> parameters;
        public IEnumerable<T> files;
        public RawTask<U> With<U>(IEnumerable<U> files)
        {
            return new RawTask<U>
            {
                parameters = this.parameters,
                files = files,
                id = this.id
            };
        }
    }
    public class RawTaskGroup
    {
        public Executor executor;
        public List<RawTask<(string, FileGetter)>> subtasks;
        public IDictionary<string, string> parameters;

    }
}