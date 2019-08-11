using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Agent
{
    public class AppConfigurationServices
    {
        public static IConfiguration Configuration { get; set; }
        public static string FileProvider { get => Configuration["fileProvider"]; }
        public static string Server { get => Configuration["server"]; }
        public static int Timeout { get => Int32.Parse(Configuration["timeout"]); }
        static AppConfigurationServices()
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }
    }
}