using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Client
{
    public class AppConfigurationServices
    {
        private static string GetKey(string key, string defaultValue = null)
            => Environment.GetEnvironmentVariable("WN_CLIENT_" + key) ?? Configuration[key] ?? defaultValue;
        public static IConfiguration Configuration { get; set; }


        public static string RabbitMQ { get => GetKey("rabbitMQ", "localhost"); }
        public static int RabbitMQPort { get => Int32.Parse(GetKey("rabbitMQPort", "5672")); }
        public static string RabbitMQUsername { get => GetKey("rabbitMQUsername", "guest"); }
        public static string RabbitMQPassword { get => GetKey("rabbitMQPassword", "guest"); }

        static AppConfigurationServices()
        {
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            string basePath = null;
            if (File.Exists("./worknet.json"))
            {
                basePath = Directory.GetCurrentDirectory();
            }
            else if (File.Exists($"{homePath}/worknet.json"))
            {
                basePath = homePath;
            }
            else
            {
                basePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            }
            Configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .Add(new JsonConfigurationSource { Path = "worknet.json", ReloadOnChange = false })
                .Build();
        }
    }
}