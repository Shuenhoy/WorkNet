using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Encodings;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Core;
using LanguageExt.Parsec;
using System.Collections.Concurrent;

using static LanguageExt.Prelude;
using static LanguageExt.List;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

using System.Text.Json;
using System.Text.Json.Serialization;

using WorkNet.Client;
using WorkNet.Common.Models;

namespace WorkNet.Client.Commands
{
    abstract class Argument { }
    class RangeArgument<T> : Argument
    {
        public T Start;
        public T End;
        public T Step;
        public bool Close;
    }

    class IterArgument : Argument
    {
        public Seq<string> Iter;
    }
    class PlainArgument : Argument
    {
        public string Arg;
    }
    enum ArgumentType
    {
        Plain,
        Argument,
        File
    }
    static class ArgumentParser
    {

        static Parser<string> executable = asString(many1(noneOf(" ")));
        static Parser<(string name, ArgumentType type)> formalArgument =
            from _0 in ch('{')
            from file in optional(ch('@'))
            from name in asString(many1(choice(letter, digit, ch('_'))))
            from _1 in ch('}')
            select (name, file.IsSome ? ArgumentType.File : ArgumentType.Argument);
        static Parser<(string name, ArgumentType type)> plain =
            from value in asString(many1(noneOf(" {}")))
            select (value, ArgumentType.Plain);

        static Parser<Unit> spaces1 = skipMany1(space);
        static Parser<(string executable, List<(string name, ArgumentType type)> arguments)> command =
            from exec in executable
            from arguments in many(
                from _0 in spaces1
                from x in either(formalArgument, plain)
                select x
            )
            select (exec, arguments.ToList());
        public static (string executable, List<(string name, ArgumentType type)> arguments) ParseCommand(string input)
            => command.Parse(input).Reply.Result;

        //////////////////////////////

        static Parser<Argument> plainArgument =
            from value in asString(many1(noneOf(" []")))
            select new PlainArgument() { Arg = value } as Argument;
        static Parser<Argument> iterArgument =
            from _0 in str("?[")
            from _01 in spaces
            from arguments in sepEndBy1(asString(many1(noneOf(" ]"))), spaces1)
            from _00 in spaces
            from _1 in ch(']')
            select new IterArgument() { Iter = arguments } as Argument;
        static Parser<int> intLit =
                   from d in asString(many1(digit))
                   select Int32.Parse(d);
        static Parser<(float lit, float step)> floatLit =
           from d in asString(many1(digit))
           from _ in ch('.')
           from c in asString(many1(digit))
           select (Single.Parse(d + '.' + c), MathF.Pow(10.0f, -1 * c.Length));
        static Parser<Argument> floatRangeArgument =
            from _0 in str("%[")
            from start in floatLit
            from _1 in str("..")
            from close in optional(ch('='))
            from end in floatLit
            from step in optional(
                from _2 in ch(',')
                from s in floatLit
                select s
            )
            from _2 in ch(']')
            select new RangeArgument<float>()
            {
                Start = start.lit,
                End = end.lit,
                Close = close.IsSome,
                Step = step.IsSome ? step.Head().lit : MathF.Min(start.step, end.step)
            } as Argument;
        static Parser<Argument> intRangeArgument =
            from _0 in str("%[")
            from start in intLit
            from _1 in str("..")
            from close in optional(ch('='))
            from end in intLit
            from step in optional(
                from _2 in ch(',')
                from s in intLit
                select s
            )
            from _2 in ch(']')
            select new RangeArgument<int>()
            {
                Start = start,
                End = end,
                Close = close.IsSome,
                Step = step.IsSome ? step.Head() : 1
            } as Argument;
        static Parser<Argument> argument =
            choice(attempt(intRangeArgument), attempt(floatRangeArgument), attempt(iterArgument), plainArgument);
        static Parser<Seq<Argument>> arguments =
             from _01 in spaces
             from args in many(attempt(
                 from x in argument
                 from _ in spaces1
                 select x))
             from last in argument
             from _00 in spaces
             select args.Add(last);
        public static List<Argument> ParseArguments(string input)
            => arguments.Parse(input).Reply.Result.ToList();

    }
    public static partial class Executor
    {
        static Seq<string> GenList(int start, int end, int step, bool eq)
        {
            var ret = new List<string>();
            if (eq)
            {
                for (; start <= end; start += step)
                {
                    ret.Add(start.ToString());
                }
                return ret.ToSeq();
            }
            else
            {
                for (; start < end; start += step)
                {
                    ret.Add(start.ToString());
                }
                return ret.ToSeq();
            }
        }
        static Seq<string> GenList(float start, float end, float step, bool eq)
        {
            var ret = new List<string>();
            if (eq)
            {
                for (; start <= end; start += step)
                {
                    ret.Add(start.ToString());
                }
                return ret.ToSeq();
            }
            else
            {
                for (; start < end; start += step)
                {
                    ret.Add(start.ToString());
                }
                return ret.ToSeq();
            }
        }
        static Seq<Seq<string>> GenerateArguments(List<Argument> arguments, int pos)
        {

            var arg = arguments[pos];
            if (pos == arguments.Count - 1)
            {

                return arg switch
                {
                    PlainArgument p => Seq1(Seq1(p.Arg)),
                    IterArgument i => i.Iter.Count > 0 && i.Iter.First() == "@" ?
                        i.Iter.Skip(1).Map(x => Seq1("@" + x))
                        : i.Iter.Map(x => Seq1(x)),
                    RangeArgument<int> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => Seq1(x)),
                    RangeArgument<float> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => Seq1(x)),
                    _ => throw new NotSupportedException()

                };
            }
            else
            {
                var next = GenerateArguments(arguments, pos + 1);
                return arg switch
                {
                    PlainArgument p => next.Map(x => x.Add(p.Arg)),
                    IterArgument i => i.Iter.Count > 0 && i.Iter.First() == "@"
                        ? next.Map(x => i.Iter.Skip(1).Map(y => x.Add("@" + y))).Flatten()
                        : next.Map(x => i.Iter.Map(y => x.Add(y))).Flatten(),
                    RangeArgument<int> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => next.Map(y => y.Add(x))).Flatten(),
                    RangeArgument<float> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => next.Map(y => y.Add(x))).Flatten(),
                    _ => throw new NotSupportedException()
                };
            }
        }
        class SingleTaskP
        {
            public Dictionary<string, string> Parameters;
            public List<string> Pulls;
        }
        class SingleTask
        {
            public Dictionary<string, string> Parameters { get; set; }
            public List<int> Pulls { get; set; }
        }
        class JExecutor
        {
            public string Image { get; set; }
            public string Execution { get; set; }
            public int Executor { get; set; }
        }
        class TaskSubmit
        {
            public JExecutor executor { get; set; }
            public List<SingleTask> tasks { get; set; }
        }
        static (string, List<string>, List<string>) ProcessCommands(string executable, List<(string name, ArgumentType type)> arguments)
        {
            string pattern = executable;
            var args = new List<string>();
            var fileArgs = new List<string>();
            foreach (var a in arguments)
            {
                if (a.type == ArgumentType.Plain)
                {
                    pattern += " " + a.name;
                }
                else if (a.type == ArgumentType.Argument)
                {
                    pattern += $" {{{a.name}}}";
                    args.Add(a.name);
                }
                else
                {
                    pattern += $" {{{a.name}}}";
                    args.Add(a.name);
                    fileArgs.Add(a.name);
                }
            }
            return (pattern, args, fileArgs);
        }
        static async Task<int> SubmitToServer(TaskSubmit submit)
        {

        }
        static int SubmitFromConfig(TaskConfig content, bool rezip, bool free)
        {
            var commands = ArgumentParser.ParseCommand(content.Execution);
            var (executionPattern, arguments, fileArguments) = ProcessCommands(commands.executable, commands.arguments);
            var taskArguments = new List<List<string>>();

            foreach (var arg in content.Tasks)
            {
                var rawArgs = ArgumentParser.ParseArguments(arg);
                if (!free && rawArgs.Count != arguments.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: Expect {arguments.Count} arguments, but got {rawArgs.Count}");
                    return -1;
                }
                else if (free)
                {
                    arguments = Range(1, rawArgs.Count).Map(x => $"arg{x}").ToList();


                    foreach (var a in arguments)
                    {
                        executionPattern += $" {{{a}}}";
                    }

                    free = false;
                }
                if (rawArgs.Length() > 0)
                {
                    taskArguments.AddRange(GenerateArguments(rawArgs, 0).Map(x => x.Rev().ToList()).ToList());
                }
                else
                {
                    taskArguments.Add(new List<string>(new[] { "" }));
                }
            }
            var tasksPList = new List<SingleTaskP>();
            var allPulls = new System.Collections.Generic.HashSet<string>();
            foreach (var task in taskArguments)
            {
                var parameters = new Dictionary<string, string>();
                var pulls = new List<string>();
                foreach (var (value, name) in task.Zip(arguments))
                {
                    if (fileArguments.Contains(name))
                    {
                        pulls.Add(value);
                        allPulls.Add(value);
                        parameters[name] = value;

                    }
                    else if (value.StartsWith('@'))
                    {
                        pulls.Add(value.Substring(1));
                        allPulls.Add(value.Substring(1));
                        parameters[name] = value.Substring(1);

                    }
                    else
                    {
                        parameters[name] = value;

                    }

                }
                tasksPList.Add(new SingleTaskP() { Parameters = parameters, Pulls = pulls });
            }

            var fileToId = new ConcurrentDictionary<string, int>();
            Task.WaitAll(allPulls.Map(async x =>
            {
                int res = await Upload(x);
                fileToId[x] = res;
            }).ToArray());

            var taskList = tasksPList.Map(x => new SingleTask() { Parameters = x.Parameters, Pulls = x.Pulls.Map(y => fileToId[y]).ToList() });
            //----------------------------------------------------------------------------------------------------------------------------------
            if (!File.Exists("wn_executor.zip") || rezip)
            {
                ZipHelper.CreateFromDirectory(
                    Directory.GetCurrentDirectory(), "wn_executor.zip", CompressionLevel.Fastest, false, Encoding.UTF8,
                    fileName => !fileName.Contains(@"wn_")
                );
            }
            var upd = Upload("wn_executor.zip");
            upd.Wait();

            var submit = new TaskSubmit()
            {
                executor = new JExecutor() { Image = content.Image, Execution = executionPattern, Executor = upd.Result },
                tasks = taskList.ToList()
            };
            var upd2 = SubmitToServer(submit);
            upd2.Wait();
            content.Id = upd2.Result;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Task submit success. The id is {upd2.Result}");
            return 0;
        }
        public static int Submit(SubmitOptions opts)
        {
            if (!File.Exists(defaultFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No such file '{defaultFile}'");
                return -1;
            }
            var content = JsonSerializer.Deserialize<TaskConfig>(File.ReadAllText(defaultFile));

            int ret = SubmitFromConfig(content, opts.ReZip, false);
            File.WriteAllText(defaultFile, JsonSerializer.Serialize(content, jsonOpt));

            return ret;
        }

        public static int Run(RunOptions opts)
        {
            return SubmitFromConfig(new TaskConfig()
            {
                Image = opts.Image,
                Execution = opts.Commands.First(),
                Tasks = Seq1(String.Join(' ', opts.Commands.Skip(1))).ToList()
            }, opts.ReZip, true);
        }
    }

    public static class ZipHelper
    {
        private static string[] GetEntryNames(string[] names, string sourceFolder, bool includeBaseName)
        {
            if (names == null || names.Length == 0)
                return new string[0];

            if (includeBaseName)
                sourceFolder = Path.GetDirectoryName(sourceFolder);

            int length = string.IsNullOrEmpty(sourceFolder) ? 0 : sourceFolder.Length;
            if (length > 0 && sourceFolder != null && sourceFolder[length - 1] != Path.DirectorySeparatorChar && sourceFolder[length - 1] != Path.AltDirectorySeparatorChar)
                length++;

            var result = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                result[i] = names[i].Substring(length);
            }

            return result;
        }
        public static void CreateFromDirectory(
            string sourceDirectoryName
        , string destinationArchiveFileName
        , CompressionLevel compressionLevel
        , bool includeBaseDirectory
        , Encoding entryNameEncoding
        , Predicate<string> filter // Add this parameter
        )
        {
            if (string.IsNullOrEmpty(sourceDirectoryName))
            {
                throw new ArgumentNullException("sourceDirectoryName");
            }
            if (string.IsNullOrEmpty(destinationArchiveFileName))
            {
                throw new ArgumentNullException("destinationArchiveFileName");
            }
            var filesToAdd = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
            var entryNames = GetEntryNames(filesToAdd, sourceDirectoryName, includeBaseDirectory);
            using (var zipFileStream = new FileStream(destinationArchiveFileName, FileMode.Create))
            {
                using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    for (int i = 0; i < filesToAdd.Length; i++)
                    {
                        // Add the following condition to do filtering:
                        if (!filter(filesToAdd[i]))
                        {
                            continue;
                        }
                        archive.CreateEntryFromFile(filesToAdd[i], entryNames[i], compressionLevel);
                    }
                }
            }
        }
    }
}