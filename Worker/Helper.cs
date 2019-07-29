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
        public static async Task StopContainer(this DockerClient client, string id)
        {
            await client.Containers.StopContainerAsync(id, new ContainerStopParameters());
        }
        public static async Task RemoveContainer(this DockerClient client, string id)
        {
            await client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters());
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

            var multiplexedStream = await client.Containers.StartAndAttachContainerExecAsync(createdExec.ID, false);

            return await multiplexedStream.ReadOutputToEndAsync(CancellationToken.None);
        }
        public static Task<(string stdout, string stderr)> RunCommandInContainerAsync(this DockerClient client, string containerId, string command)
        {
            var commandTokens = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            return RunCommandInContainerAsync(client, containerId, commandTokens);
        }
    }
}