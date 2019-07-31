using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Client
{
    public class AppConfigurationServices
    {
        public static IConfiguration Configuration { get; set; }
        public static string FileProvider { get; set; }
        public static string Server { get; set; }

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
            FileProvider = Configuration["fileProvider"];
            Server = Configuration["server"];
        }
    }
}