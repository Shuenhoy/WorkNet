using Docker.DotNet;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using LanguageExt;
using SmartFormat;


using System.Linq;

namespace WorkNet.Agent.Worker
{
    class GroupInfo
    {
        public long Id { get; set; }
        public string Image { get; set; }
        public string Execution { get; set; }
        public int? Executor { get; set; }

        public List<Dictionary<string, JsonElement>> Parameters { get; set; }
        public List<int> Pulls { get; set; }
    }
    public class FileEntry
    {
        public int FileEntryID { get; set; }
        public string SeaweedId { get; set; }

        public JsonElement Metadata;

        public int Size { get; set; }
        public string ETag { get; set; }
        public List<string> Tags { get; set; }
        public string ExtName { get; set; }
        public string FileName { get; set; }
        public string Namespace { get; set; }

    }
    public class DockerWorker
    {
        DockerClient docker;
        HttpClient client;
        string server;
        string fileProvider;
        string workDir;
        public DockerWorker()
        {
            client = new HttpClient();
            docker = new DockerClientConfiguration(
                   new Uri("unix:///var/run/docker.sock"))
               .CreateClient();
            workDir = Directory.GetCurrentDirectory();
            server = AppConfigurationServices.Configuration["server"];
            fileProvider = AppConfigurationServices.Configuration["fileProvider"];
        }


        public async Task ExecTaskGroup(int id)
        {
            Console.WriteLine($"{server}/api/tasks/@group/{id}");
            var info = await client
                .GetAsync($"{server}/api/tasks/@group/{id}")
                .Bind(x => x.Content.ReadAsStringAsync())
                .Map(x =>
                {
                    Console.WriteLine(x);
                    return JsonSerializer.Deserialize<GroupInfo>(x, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                );

            string containerId = null;
            Console.WriteLine(info.Executor);
            await PullFiles(info.Pulls, info.Executor);
            Directory.CreateDirectory("data/out");

            try
            {
                containerId = await docker.CreateContainer(info.Image, new string[]{
                    $"{workDir}/data/pulls:/app/wn_pulls",
                    $"{workDir}/data/out:/app/wn_out",
                    $"{workDir}/data/app:/app"

                });
                {
                    await docker.RunCommandInContainerAsync(containerId,
                            new[] { "sh", "init.sh" });
                }
                int index = 0;
                var results = new List<int>();
                foreach (var parameter in info.Parameters)
                {
                    Directory.CreateDirectory("data/out");

                    var (stdout, stderr) = await docker.RunCommandInContainerAsync(containerId,
                        new[] { "sh", "-c", Smart.Format(info.Execution, parameter)
                    });
                    Task.WaitAll(File.WriteAllTextAsync("data/out/wn_stdout.txt", Smart.Format(info.Execution, parameter) + "\n" + stdout), File.WriteAllTextAsync("data/out/wn_stderr.txt", stderr));
                    // Submit Result
                    results.Add(await Sumbit(info.Id, index));
                    Directory.Delete("data/out", true);
                    index++;
                }
                var respup = await client.PostAsync($"{server}/api/tasks/result/{info.Id}", new StringContent(JsonSerializer.Serialize(results), Encoding.UTF8, "application/json"));
                respup.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
            finally
            {
                try
                {
                    await docker.StopContainer(containerId);
                }
                finally
                {
                    await docker.RemoveContainer(containerId);
                    RemoveFiles();
                }
            }
        }
        async Task<int> Sumbit(long groupId, int singleId)
        {

            File.Delete($"data/results/result_{groupId}_{singleId}.zip");
            ZipFile.CreateFromDirectory("data/out", $"data/results/result_{groupId}_{singleId}.zip");
            using var content = new MultipartFormDataContent();
            using var stream = new FileStream($"data/results/result_{groupId}_{singleId}.zip", FileMode.Open, FileAccess.Read);
            content.Add(new StringContent(JsonSerializer.Serialize(new { Namespace = "__worknet_result" })), "payload");
            content.Add(new StreamContent(stream), "files", $"result_{groupId}_{singleId}.zip");

            var resp = await client.PostAsync($"{fileProvider}/api/file", content);
            var ret = await resp.Content.ReadAsStringAsync();
            var file = JsonSerializer.Deserialize<FileEntry>(ret, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return file.FileEntryID;
        }
        void RemoveFiles()
        {
            Directory.Delete("data/pulls", true);
            Directory.Delete("data/app", true);
            Directory.Delete("data/results", true);


        }
        async Task PullFiles(List<int> pulls, int? executor)
        {
            Directory.CreateDirectory("data/pulls");
            Directory.CreateDirectory("data/results");
            Directory.CreateDirectory("data/app");


            if (executor.HasValue)
            {
                int exec = executor.Value;
                var resp2 = await client.GetAsync($"{fileProvider}/api/file/@id/{exec}");
                var stream = await resp2.Content.ReadAsStreamAsync();
                var s = new FileStream("data/pulls/__wn_executor.tar", FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(s);
                s.Close();
                ZipFile.ExtractToDirectory("./data/pulls/__wn_executor.tar", "./data/app");
            }

            try
            {
                Task.WaitAll(pulls
                    .Map(async pull =>
                    {
                        var resp = await client.GetAsync($"{fileProvider}/api/file/@idhead/{pull}");


                        var src = await resp.Content.ReadAsStringAsync();
                        var fileInfo = JsonSerializer.Deserialize<FileEntry>(src, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        var resp2 = await client.GetAsync($"{fileProvider}/api/file/@id/{fileInfo.FileEntryID}");
                        var stream = await resp2.Content.ReadAsStreamAsync();
                        using var s = new FileStream($"data/pulls/{fileInfo.FileName}", FileMode.Create, FileAccess.Write);
                        await stream.CopyToAsync(s);

                    }).ToArray());
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
    }
}