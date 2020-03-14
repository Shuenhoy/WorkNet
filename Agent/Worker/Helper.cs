using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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
            var image = split.Length == 1 ? split.First() : String.Join(":", split.Take(split.Length - 1));
            var tag = split.Length > 1 ? split.Last() : "latest";
            Console.WriteLine("try to pull image: " + name);
            await client.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = image,
                Tag = tag
            }, null, new Progress<JSONMessage>(resp =>
            {
                Console.WriteLine(resp.Status);
            }));
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
            this DockerClient client, string image, string workDir, string[] binds = null)
        {
            var ret = await client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = image,
                AttachStdin = true,
                AttachStderr = true,
                AttachStdout = true,
                OpenStdin = true,
                Tty = true,
                WorkingDir = workDir,
                HostConfig = new HostConfig()
                {
                    Binds = binds,
                    CPUPeriod = AppConfigurationServices.CpuPeriod,
                    CPUQuota = AppConfigurationServices.CpuQuota,
                    Memory = AppConfigurationServices.Memory,
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
        public static async Task<(string stdout, string stderr, long exitCode)> RunCommandInContainerAsync(this DockerClient client, string containerId, string[] commandTokens)
        {
            //await client.Containers.StartAndAttachContainerExecAsync()
            var createdExec = await client.Containers.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
            {
                AttachStderr = true,
                AttachStdout = true,
                Cmd = commandTokens
            });
            var multiplexedStream = await client.Containers.StartAndAttachContainerExecAsync(createdExec.ID, false); ;
            var insp = await client.Containers.InspectContainerExecAsync(createdExec.ID);

            var task = multiplexedStream.ReadOutputToEndAsync(CancellationToken.None);
            if (await Task.WhenAny(task, Task.Delay(AppConfigurationServices.Timeout)) != task)
            {

                throw new TimeoutException() { Commands = String.Join(" ", commandTokens) };

            }
            return (task.Result.stdout, task.Result.stderr, insp.ExitCode);
        }
        public static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }
        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;
            bool isEscaping = false;

            return commandLine.Split(c =>
            {
                if (c == '\\' && !isEscaping) { isEscaping = true; return false; }

                if (c == '\"' && !isEscaping)
                    inQuotes = !inQuotes;

                isEscaping = false;

                return !inQuotes && Char.IsWhiteSpace(c)/*c == ' '*/;
            })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"').Replace("\\\"", "\""))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }
        public static Task<(string stdout, string stderr, long exitCode)> RunCommandInContainerAsync(this DockerClient client, string containerId, string command)
        {
            var commandTokens = SplitCommandLine(command);
            return RunCommandInContainerAsync(client, containerId, commandTokens.ToArray());
        }
    }
}
