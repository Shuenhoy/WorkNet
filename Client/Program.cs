using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using WorkNet.Client.Commands;
namespace WorkNet.Client
{
    [Verb("add", HelpText = "Add several single tasks")]
    public class AddOptions
    {
        [Value(0)]
        public IEnumerable<string> Commands { get; set; }
    }
    [Verb("submit", HelpText = "Submit to the server")]
    public class SubmitOptions
    {
        [Option('r', "rezip", Default = false, HelpText = "rezip the executor files")]
        public bool ReZip { get; set; }
    }
    [Verb("run", HelpText = "Add tasks and submit to the server")]
    public class RunOptions
    {
        [Option('r', "rezip", Default = false, HelpText = "rezip the executor files")]
        public bool ReZip { get; set; }
        [Option('i', "image", Required = false, Default = "ubuntu:18.04", HelpText = "Set the runtime image")]
        public string Image { get; set; }
        [Value(0, Min = 1)]
        public IEnumerable<string> Commands { get; set; }
    }
    [Verb("init", HelpText = "Create a task")]
    public class InitOptions
    {
        [Option('f', "file", Required = false, Default = "WNfile.json", HelpText = "Set the configuration file")]
        public string File { get; set; }
        [Option('i', "image", Required = false, Default = "ubuntu:18.04", HelpText = "Set the runtime image")]
        public string Image { get; set; }
        [Value(0, Min = 1)]
        public IEnumerable<string> Commands { get; set; }
    }

    [Verb("pull", HelpText = "Pull Results")]
    public class PullOptions
    {
        [Option('i', "id", HelpText = "Set the id to pull")]
        public int? Id { get; set; }
    }
    class Program
    {


        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<AddOptions, SubmitOptions, InitOptions, PullOptions, RunOptions>(args)
              .MapResult(
                (InitOptions opts) => Executor.Init(opts),
                (AddOptions opts) => Executor.Add(opts),
                (SubmitOptions opts) => Executor.Submit(opts),
                (PullOptions opts) => Executor.Pull(opts),
                (RunOptions opts) => Executor.Run(opts),

            errs => 1);
        }
    }
}
