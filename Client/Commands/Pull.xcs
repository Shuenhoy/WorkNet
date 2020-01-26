using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkNet.Client;
using System.Collections.Generic;
using System.Runtime;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using LanguageExt;
using System.Linq;
using WorkNet.Common.Models;

namespace WorkNet.Client.Commands
{


    public static partial class Executor
    {

        public static int Pull(PullOptions opts)
        {
            int id = 0;
            if (opts.Id.HasValue)
            {
                id = opts.Id.Value;
            }
            else
            {
                if (!File.Exists(defaultFile))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: No such file '{defaultFile}'");
                    return -1;
                }
                var content = JsonSerializer.Deserialize<TaskConfig>(File.ReadAllText(defaultFile), jsonOpt);
                if (!content.Id.HasValue)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: The task has not been submitted");
                    return -1;
                }
                id = content.Id.Value;
            }

            var tsk = client.GetAsync($"{AppConfigurationServices.Server}/api/tasks/{id}")
                .Bind(x => x.Content.ReadAsStringAsync())
                .Map(x =>
                {

                    return JsonSerializer.Deserialize<UserTask>(x, jsonOpt);
                });
            tsk.Wait();
            if (tsk.Result.SubFinished != tsk.Result.SubTotal)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"INFO: The task is in process. {tsk.Result.SubFinished}/{tsk.Result.SubTotal} done");
                return 0;
            }
            var pulls = tsk.Result.SubTasks.Map(x =>
            {
                if (x.Status == TaskGroupStatus.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"INFO: The task group {x.TaskGroupId} ends in error: {x.ErrorMessage}");
                    return Enumerable.Empty<int>();
                }
                else
                {
                    return x.SingleTasks.Map(y => y.Result.Value);
                }
            }).Flatten().ToList();

            Directory.CreateDirectory("wn_result");
            Directory.CreateDirectory($"wn_result/{id}");
            Task.WaitAll(pulls
                    .Map(async pull =>
                    {
                        var resp = await client.GetAsync($"{AppConfigurationServices.FileProvider}/api/file/@idhead/{pull}");


                        var src = await resp.Content.ReadAsStringAsync();
                        var fileInfo = JsonSerializer.Deserialize<FileEntry>(src, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        var resp2 = await client.GetAsync($"{AppConfigurationServices.FileProvider}/api/file/@id/{fileInfo.FileEntryID}");
                        var stream = await resp2.Content.ReadAsStreamAsync();
                        var zipFile = $"wn_result/{id}/{fileInfo.FileName}";
                        var s = new FileStream(zipFile, FileMode.Create, FileAccess.Write);
                        await stream.CopyToAsync(s);
                        var outputDir = $"wn_result/{id}/{Path.GetFileNameWithoutExtension(fileInfo.FileName)}";
                        Directory.CreateDirectory(outputDir);
                        s.Close();
                        ZipFile.ExtractToDirectory(zipFile, outputDir);
                        File.Delete(zipFile);

                    }).ToArray());
            return 0;
        }
    }
}