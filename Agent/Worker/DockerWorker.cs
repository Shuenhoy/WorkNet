using Docker.DotNet;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using LanguageExt;
using SmartFormat;
using WorkNet.Common.Models;
using WorkNet.Common;
using MoonSharp.Interpreter;
using System.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace WorkNet.Agent.Worker
{
    public class LuaAgent
    {
        DockerClient docker;
        Dictionary<string, string> containers;
        string workDir;
        ILogger logger;

        public LuaAgent(DockerClient docker, string workDir, ILogger logger)
        {
            this.docker = docker;
            this.workDir = workDir;
            this.logger = logger;
            this.containers = new Dictionary<string, string>();
        }

        public FilePair AddFile(string path)
        {
            return new FilePair()
            {
                filename = path,
                file = new FileBytes(File.ReadAllBytes($"data/out/{path}"))
            };
        }
        internal void cleanup()
        {
            Task.WaitAll(containers.Values.Map(x => docker.RemoveContainer(x)).ToArray());
            containers.Clear();
        }
        private string getContainer(string image)
        {
            try
            {
                if (!containers.ContainsKey(image))
                {
                    docker.PullImage(image).Wait();
                    var c = docker.CreateContainer(image, "/app", new string[]{
                        $"{workDir}/data/pulls:/app/wn_pulls",
                        $"{workDir}/data/out:/app/wn_out",
                        $"{workDir}/data/app:/app"
                    });
                    c.Wait();
                    logger.LogInformation($"container {c.Result}({image}) created.");
                    containers[image] = c.Result;
                }
                return containers[image];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
        public long Docker(string image, string command, out string stdout, out string stderr)
        {
            var container = getContainer(image);
            logger.LogInformation($"execute in {container}({image}): {command}");
            var tsk = docker.RunCommandInContainerAsync(container,
                                        command);
            tsk.Wait();
            stdout = tsk.Result.stdout;
            stderr = tsk.Result.stderr;
            return tsk.Result.exitCode;
        }
        public string Format(string format, Dictionary<string, object> arguments)
        {
            return Smart.Format(format, arguments);
        }
        public long DockerArr(string image, string[] commands, out string stdout, out string stderr)
        {
            var container = getContainer(image);
            logger.LogInformation($"execute in {container}({image}): {String.Join(" ", commands)}");
            var tsk = docker.RunCommandInContainerAsync(container,
                                        commands);
            tsk.Wait();

            stdout = tsk.Result.stdout;
            stderr = tsk.Result.stderr;
            logger.LogInformation(stdout);
            return tsk.Result.exitCode;
        }
        public FilePair AddPath(string path)
        {
            var self = $"data/out/wn_prefix_{path}_wn.zip";
            ZipHelper.CreateFromDirectory(
                $"data/out/{path}",
                 $"data/out/wn_prefix_{path}_wn.zip", CompressionLevel.Fastest, false, Encoding.UTF8,
                    path => path != self);
            var ret = new FilePair()
            {
                filename = path + ".zip",
                file = new FileBytes(File.ReadAllBytes($"data/out/wn_prefix_{path}_wn.zip"), true)
            };
            return ret;

        }

    }
    public class DockerWorker
    {


        Script state;
        LuaAgent agent;
        ILogger logger;
        IModel channel;
        string workDir;
        DockerClient getDocker()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new DockerClientConfiguration(
                                   new Uri("npipe://./pipe/docker_engine"))
                               .CreateClient();
            else return new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        }
        public DockerWorker(IModel c, ILogger l)
        {
            logger = l;
            workDir = AppConfigurationServices.WorkDir;
            agent = new LuaAgent(getDocker(), workDir, logger);
            UserData.RegisterType<LuaAgent>();
            UserData.RegisterType<FilePair>();
            state = new Script();

            state.Globals["agent"] = agent;
            state.DoString(@"
                local a = agent
                local unpack = table.unpack
                local print = print
                local String = String
                function file(...)
                    return a:AddFile(unpack({...}))
                end
                function folder(...)
                    return a:AddPath(unpack({...}))
                end
                function docker(...)
                    return a:Docker(unpack({...}))
                end
                function docker_arr(...)
                    return a:DockerArr(unpack({...}))
                end
                function format(...)
                    return a:Format(unpack({...}))
                end
                function run(untrusted_code, env)
                    local untrusted_function, message = load(untrusted_code, nil, 't', env)
                    if not untrusted_function then return nil, message end
                    return pcall(untrusted_function)
                end
            ");

            channel = c;
        }
        object ToObj(DynValue obj)
        {
            return obj.Type switch
            {
                DataType.Boolean => obj.Boolean,
                DataType.String => obj.String,
                DataType.Number => obj.Number,
                DataType.UserData => obj.UserData.Object,
                DataType.Nil => null,
                DataType.Table => obj.Table.Pairs.ToDictionary(kv => ToObj(kv.Key), kv => ToObj(kv.Value)),
                _ => throw new NotImplementedException()
            };
        }

        public async Task ExecTaskGroup(TaskGroup task, BasicDeliverEventArgs ea)
        {


            try
            {

                RemoveFiles();
                logger.LogInformation($"task from {ea.BasicProperties.ReplyTo}");

                CreateDirectories();
                await WriteFiles(task.files, task.executor.worker);

                state.Globals["global"] = task.parameters;
                state.Globals["init"] = true;
                foreach (var subtask in task.subtasks)
                {
                    try
                    {
                        state.Globals["task"] = subtask.parameters;
                        state.Globals["source"] = task.executor.source;
                        Directory.CreateDirectory($"data/out");

                        var raw = state.DoString(
                            @"return run(source, {global = global, 
                                        task = task, file = file, folder = folder, docker_arr = docker_arr,
                                        docker = docker, format = format, init = init})");
                        if (raw.Tuple[0].Boolean == false)
                        {
                            logger.LogError(raw.Tuple[1].String);
                            throw new Exception("Lua Exception:\n" + raw.Tuple[1].String);
                        }
                        var ret = ((Dictionary<object, object>)ToObj(raw.Tuple[1]))
                            .ToDictionary(kv => kv.Key as string, kv =>
                                kv.Value switch
                                {
                                    FilePair fp => fp as PayloadItem,
                                    _ => new ObjectPayload() { obj = kv.Value } as PayloadItem
                                });

                        string a, b;
                        agent.DockerArr("alpine", new[] { "sh", "-c", "rm -rf wn_out/*" }, out a, out b);
                        var status = raw.Tuple[2].CastToString();
                        if (status != "failed" && status != "invalid")
                        {
                            status = "result";
                        }

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.Type = status;
                        properties.MessageId = subtask.id.ToString();
                        properties.CorrelationId = ea.BasicProperties.CorrelationId;

                        channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: properties,
                         body: new TaskResult() { payload = ret, id = subtask.id }.SerializeToByteArray());
                        state.Globals["init"] = false;
                        logger.LogInformation("subtask done");
                    }
                    catch (Exception ae)
                    {
                        var error = ae.Message;
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.Type = "failed";
                        properties.MessageId = subtask.id.ToString();
                        properties.CorrelationId = ea.BasicProperties.CorrelationId;

                        channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: properties,
                         body: new TaskResult()
                         {
                             payload = new Dictionary<string, PayloadItem> { ["_error"] = new ObjectPayload() { obj = error as object } as PayloadItem },
                             id = subtask.id
                         }.SerializeToByteArray());
                        state.Globals["init"] = false;
                        logger.LogError("subtask failed.\n" + error);
                    }

                }

            }
            catch (AggregateException ae)
            {
                var error = "";
                foreach (var e in ae.InnerExceptions)
                {
                    error += e.ToString();
                }
                logger.LogError("task failed.\n" + error);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            finally
            {
                try
                {
                    agent.cleanup();
                }
                finally
                {
                    RemoveFiles();
                }
            }


        }
        void RemoveFiles()
        {
            try
            {
                Directory.Delete($"data/pulls", true);
                Directory.Delete($"data/app", true);
                Directory.Delete($"data/out", true);
            }
            catch (Exception) { }

        }
        void CreateDirectories()
        {
            Directory.CreateDirectory($"data/pulls");
            Directory.CreateDirectory($"data/out");
            Directory.CreateDirectory($"data/app");
        }
        async Task WriteFiles(List<(string fileName, FileGetter file)> files, FileGetter executor)
        {
            if (executor != null)
            {
                await executor.WriteTo($"data/pulls/__wn_executor.zip");
                ZipFile.ExtractToDirectory($"data/pulls/__wn_executor.zip", "./data/app");
            }
            try
            {
                Task.WaitAll(files
                    .Map(file =>
                    {
                        return file.file.WriteTo($"data/pulls/{file.fileName}");
                    }).ToArray());
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
    }
}