using Docker.DotNet;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace WorkNet.Agent.Worker
{
    class GroupInfo
    {
        public long Id { get; set; }
        public List<JsonElement> Parameters { get; set; }
        public List<string> Pulls { get; set; }
    }
    public class DockerWorker
    {
        DockerClient docker;
        HttpClient client;
        string server;
        public DockerWorker()
        {
            client = new HttpClient();
            docker = new DockerClientConfiguration(
                   new Uri("unix:///var/run/docker.sock"))
               .CreateClient();
            server = AppConfigurationServices.Configuration["server"];
        }
        public async Task ExecTaskGroup(int id)
        {



            await docker.Containers.StartContainerAsync("helloworld", null);
        }
    }
}