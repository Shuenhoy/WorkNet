using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Agent
{
    public class AppConfigurationServices
    {
        private static string GetKey(string key)
            => Environment.GetEnvironmentVariable("WN_AGENT_"+key) ?? Configuration[key];
        public static IConfiguration Configuration { get; set; }
        public static string FileProvider { get => GetKey("fileProvider"); }
        public static string RabbitMQ {get => GetKey("rabbitMQ");}
        public static string Server { get => GetKey("server"); }
        public static int Timeout { get => Int32.Parse(GetKey("timeout")); }
        public static int CpuPeriod { get => Int32.Parse(GetKey("cpuPeriod")); }
        public static int CpuQuota { get => Int32.Parse(GetKey("cpuQuota")); }
        public static long Memory { get => Int64.Parse(GetKey("memory")); }
        public static string WorkDir { get => GetKey("workDir"); }
        static AppConfigurationServices()
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true, Optional=true })
            .Build();
        }
    }
}