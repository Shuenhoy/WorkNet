using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkNet.Agent.Worker
{
    class Defer : IDisposable
    {
        private readonly Action _disposal;
        static public Defer defer(Action disposal)
        {
            return new Defer(disposal);
        }
        public Defer(Action disposal)
        {
            _disposal = disposal;
        }

        void IDisposable.Dispose()
        {
            _disposal();
        }
    }
    static class DockerHelperExtension
    {
        public static async Task PullImage(this DockerClient client, string name)
        {
            var split = name.Split(':');
            if (split.Length > 2) throw new Exception("incorrect image format!");
            var image = split[0];
            var tag = split.Length > 1 ? split[1] : "latest";
            Console.WriteLine("try to pull image: " + name);
            client.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = image,
                Tag = tag
            }, null, new Progress<JSONMessage>(resp =>
            {
                Console.WriteLine(resp.Status);
            })).Wait();
            Console.WriteLine("done");
        }
        public static async Task StopContainer(this DockerClient client, string id)
        {
            await client.Containers.StopContainerAsync(id, new ContainerStopParameters());
        }
        public static async Task RemoveContainer(this DockerClient client, string id)
        {
            await client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters() { Force = true });
        }
        public static async Task<string> CreateContainer(
            this DockerClient client, string image, string[] binds = null)
        {
            var ret = await client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = image,
                AttachStdin = true,
                AttachStderr = true,
                AttachStdout = true,
                OpenStdin = true,
                WorkingDir = "/app",
                HostConfig = new HostConfig()
                {
                    Binds = binds
                }
            });
            Console.WriteLine(ret.ToString());

            await client.Containers.StartContainerAsync(ret.ID, new ContainerStartParameters());
            return ret.ID;
        }
        public static async Task<MultiplexedStream> AttachContainer(this DockerClient client, string ID)
        {
            return await client.Containers.AttachContainerAsync(ID, false,
                new ContainerAttachParameters()
                {
                    Stream = true,
                    Stderr = true,
                    Stdin = false,
                    Stdout = true,
                });
        }
        public static async Task WaitContainer(this DockerClient client, string ID)
        {
            await client.Containers.WaitContainerAsync(ID);
        }
        public static async Task<(string stdout, string stderr)> RunCommandInContainerAsync(this DockerClient client, string containerId, string[] commandTokens)
        {
            //await client.Containers.StartAndAttachContainerExecAsync()
            var createdExec = await client.Containers.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
            {
                AttachStderr = true,
                AttachStdout = true,

                Cmd = commandTokens
            });


            var multiplexedStream = await client.Containers.StartAndAttachContainerExecAsync(createdExec.ID, false); ;

            var task = multiplexedStream.ReadOutputToEndAsync(CancellationToken.None);
            if (await Task.WhenAny(task, Task.Delay(AppConfigurationServices.Timeout)) != task)
            {

                throw new TimeoutException() { Commands = String.Join(" ", commandTokens) };

            }
            return task.Result;
        }
        public static Task<(string stdout, string stderr)> RunCommandInContainerAsync(this DockerClient client, string containerId, string command)
        {
            var commandTokens = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            return RunCommandInContainerAsync(client, containerId, commandTokens);
        }
    }
}
