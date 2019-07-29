using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace WorkNet.Agent
{
    public class AppConfigurationServices
    {
        public static IConfiguration Configuration { get; set; }
        static AppConfigurationServices()
        {
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }
    }
}