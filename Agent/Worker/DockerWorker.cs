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
using NLua;
using System.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        public FileGetter AddFile(string path)
        {
            return new FileBytes(File.ReadAllBytes(path));
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
                    $"{workDir}/pulls:/app/wn_pulls",
                    $"{workDir}/out:/app/wn_out",
                    $"{workDir}/app:/app"

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
        public (string stdout, string stderr, long exitCode) Docker(string image, string command)
        {
            var container = getContainer(image);
            logger.LogInformation($"execute in {container}({image}): {command}");
            var tsk = docker.RunCommandInContainerAsync(container,
                                        command);
            tsk.Wait();
            return tsk.Result;
        }
        public string Format(string format, Dictionary<string, object> arguments)
        {
            return Smart.Format(format, arguments);
        }
        public (string stdout, string stderr, long exitCode) Docker(string image, string[] commands)
        {
            var container = getContainer(image);
            logger.LogInformation($"execute in {container}({image}): {String.Join(" ", commands)}");
            var tsk = docker.RunCommandInContainerAsync(container,
                                        commands);
            tsk.Wait();
            return tsk.Result;
        }
        public FileGetter AddPath(string path)
        {
            ZipFile.CreateFromDirectory(path, path + ".zip");
            return new FileBytes(File.ReadAllBytes(path + ".zip"));

        }

    }
    public class DockerWorker
    {


        Lua state;
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

            state = new Lua();
            state["agent"] = agent;
            state.LoadCLRPackage();
            state.DoString(@"
                import 'System'
                local a = agent
                local unpack = table.unpack
                local print = print
                local String = String
                local luanet = luanet
                function file(...)
                    return a:AddFile(unpack({...}))
                end
                function folder(...)
                    return a:AddPath(unpack({...}))
                end
                function docker(...)
                    local ret = a:Docker(unpack({...}))
                    return ret.Item1, ret.Item2, ret.Item3
                end
                function docker_arr(image, arr)
                    local ret = a:Docker(image, luanet.make_array(String, arr))
                    return ret.Item1, ret.Item2, ret.Item3
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

        public async Task ExecTaskGroup(TaskGroup task, BasicDeliverEventArgs ea)
        {


            try
            {

                RemoveFiles();
                logger.LogInformation($"task from {ea.BasicProperties.ReplyTo}");

                CreateDirectories();
                await WriteFiles(task.files, task.executor.worker);

                state["global"] = task.parameters;
                foreach (var subtask in task.subtasks)
                {
                    state["task"] = subtask.parameters;
                    state["source"] = task.executor.source;
                    logger.LogInformation(task.executor.source);
                    Directory.CreateDirectory($"{workDir}/out");

                    var raw = state.DoString(
                        @"return run(source, {global = global, 
                                        task = task, file = file, folder = folder, docker_arr = docker_arr,
                                        docker = docker, format = format})");
                    if ((bool)(raw[0]) == false)
                    {
                        logger.LogError(raw[1].ToString());
                        throw new Exception("Lua Exception:\n" + (string)raw[1]);
                    }
                    var ret = state.GetTableDict((LuaTable)raw[1]).ToDictionary(x => x.Key.ToString(), x => x.Value);
                    foreach (var (k, v) in ret)
                    {
                        Console.WriteLine($"{k} = {v}");
                    }
                    Directory.Delete($"{workDir}/out", true);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Type = "result";

                    channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: properties,
                     body: new TaskResult() { payload = ret, id = subtask.id }.SerializeToByteArray());
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
                Directory.Delete($"{workDir}/pulls", true);
                Directory.Delete($"{workDir}/app", true);
                Directory.Delete($"{workDir}/results", true);
            }
            catch (Exception) { }

        }
        void CreateDirectories()
        {
            Directory.CreateDirectory($"{workDir}/pulls");
            Directory.CreateDirectory($"{workDir}/results");
            Directory.CreateDirectory($"{workDir}/app");
        }
        async Task WriteFiles(List<(string fileName, FileGetter file)> files, FileGetter executor)
        {
            if (executor != null)
            {
                await executor.WriteTo("./data/pulls/_wn_exexecutor.zip");
                ZipFile.ExtractToDirectory("./data/pulls/__wn_executor.zip", "./data/app");
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