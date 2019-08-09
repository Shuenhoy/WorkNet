using System;

namespace WorkNet.Agent.Worker
{
    class TimeoutException : Exception
    {
        public string Commands { get; set; }
        public override string ToString()
        {
            return $"Command <{Commands}> runs into timeout";
        }
    }
}