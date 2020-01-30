using WorkNet.Common.Models;
using WorkNet.Common;
using RabbitMQ.Client;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using LanguageExt;
using System.IO.Compression;
using System.Text;

using static LanguageExt.Prelude;

namespace WorkNet.Client.Submit
{
    public static class Submitter
    {
        public static int Submit(IModel channel, RunOptions opt, ITaskDivider divider)
        {
            var (subs, global) = ArgsToRawTasks(opt.Commands);
            var subsWithFile = LoadFiles(subs);
            var source = File.ReadAllText(opt.Executor);
            if (!File.Exists("wn_executor.zip") || opt.ReZip == true)
                ZipHelper.CreateFromDirectory(
                    ".", "wn_executor.zip",
                    CompressionLevel.Fastest, false, Encoding.UTF8,
                    path => !path.Contains("/wn_") && !path.Contains("\\wn_"));
            var executor = new Executor()
            {
                source = source,
                worker = new FileBytes(File.ReadAllBytes("wn_executor.zip"))
            };
            SubmitRawTask(channel, new RawTaskGroup()
            {
                executor = executor,
                subtasks = subsWithFile.ToList(),
                parameters = global
            }, divider, opt.Id);
            return 0;
        }

        public static IEnumerable<RawTask<(string, FileGetter)>> LoadFiles(
            IEnumerable<RawTask<string>> subs)
        {
            var p = Task.WhenAll(subs
                .Map(x => x.files)
                .Flatten()
                .Distinct()
                .Map(filename =>
                    File
                        .ReadAllBytesAsync(filename)
                        .Map(bytes => (filename, bytes))
                )).Map(x => x.ToDictionary(
                        kv => kv.filename,
                        kv => new FileBytes(kv.bytes) as FileGetter));
            p.Wait();
            var files = p.Result;
            return subs.Map(x => x.With<(string, FileGetter)>(x.files.Map(y => (y, files[y]))));
        }
        static
         (IEnumerable<RawTask<string>> subs, IDictionary<string, string> global) ArgsToRawTasks(
             IEnumerable<string> args)
        {
            var division = args
            .Map(ArgumentParser.ParseArguments)
            .ToLookup(x => x.key.StartsWith("~"));
            var global = division[true]
                .ToDictionary(
                    x => x.key.Substring(1),
                    x => (x.arg as PlainArgument).Arg);
            var acc = 1;
            var localPrimary = division[false]
                .Map(x =>
                {
                    object key = x.key;
                    if (x.key == "")
                    {
                        key = acc;
                        acc++;
                    }
                    return (key, x.arg);

                }).ToList();
            var localKeys = localPrimary.Map(k => k.key);
            var subs = ArgumentsGenerator
                .GenerateArguments(localPrimary.Map(k => k.arg).ToList(), 0)
                .Map(x => x.Reverse()
                    .Zip(localKeys)
                    .Fold((Seq<string>(), Seq<(string value, object key)>()),
                            (state, kv) => kv.Item1.StartsWith("@") ?
                                (state.Item1.Add(kv.Item1.Substring(1)),
                                    state.Item2.Add((kv.Item1.Substring(1), kv.Item2)))
                                : (state.Item1, state.Item2.Add((kv.Item1, kv.Item2)))

                        ))
                .Map(x => new RawTask<string>()
                {
                    files = x.Item1.ToList(),
                    parameters = x.Item2.ToDictionary(y => y.key, y => y.value)
                });
            foreach (var e in subs)
            {
                foreach (var (k, v) in e.parameters)
                {
                    Console.WriteLine($"{k}={v}");
                }
            }
            return (subs.ToList(), global);
        }
        public static void SubmitRawTask(IModel channel, RawTaskGroup task, ITaskDivider divider, string id)
        {
            SubmitTasks(channel, divider.Divide(task), id);
        }
        static void SubmitTasks(IModel channel, IEnumerable<TaskGroup> tasks, string queue = "")
        {

            var xargs = new Dictionary<string, object>();
            xargs.Add("x-message-ttl", 1000 * 60 * 60 * 24 * 3);
            var rev = channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: xargs).QueueName;
            Console.WriteLine($"Publishing tasks to queue {rev}");
            channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ReplyTo = rev;
            properties.CorrelationId = Guid.NewGuid().ToString();
            Console.WriteLine($"group id: {properties.CorrelationId}");
            long total = 0;
            foreach (var task in tasks)
            {
                channel.BasicPublish(exchange: "",
                    routingKey: "task_queue", basicProperties: properties,
                              body: task.SerializeToByteArray());
                total += task.subtasks.Count;
            }
            Console.WriteLine($"{total} subtasks in {tasks.Count()} groups have been published");


        }
    }
}