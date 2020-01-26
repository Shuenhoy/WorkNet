using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Agent
{
    public class AppConfigurationServices
    {
        private static string GetKey(string key, string defaultValue = null)
            => Environment.GetEnvironmentVariable("WN_AGENT_" + key) ?? Configuration[key] ?? defaultValue;
        public static IConfiguration Configuration { get; set; }

        public static int Timeout { get => Int32.Parse(GetKey("timeout")); }
        public static int CpuPeriod { get => Int32.Parse(GetKey("cpuPeriod")); }
        public static int CpuQuota { get => Int32.Parse(GetKey("cpuQuota")); }
        public static long Memory { get => Int64.Parse(GetKey("memory")); }
        public static string WorkDir { get => GetKey("workDir", Directory.GetCurrentDirectory()); }
        public static string RabbitMQ { get => GetKey("rabbitMQ", "localhost"); }
        public static int RabbitMQPort { get => Int32.Parse(GetKey("rabbitMQPort", "5672")); }
        public static string RabbitMQUsername { get => GetKey("rabbitMQUsername", "guest"); }
        public static string RabbitMQPassword { get => GetKey("rabbitMQPassword", "guest"); }
        static AppConfigurationServices()
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true, Optional = true })
            .Build();
        }
    }
}