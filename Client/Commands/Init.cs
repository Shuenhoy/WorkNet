using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkNet.Client;
using System.Collections.Generic;
using System.Runtime;

namespace WorkNet.Client.Commands
{

    class TaskConfig
    {
        public DateTime CreatedAt { get; set; }
        public string Image { get; set; }
        public string Execution { get; set; }
        public int? Id { get; set; }

        public List<string> Tasks { get; set; }
    }
    public static partial class Executor
    {
        static string defaultFile = Environment.GetEnvironmentVariable("wnfile") ?? "WNfile.json";
        static JsonSerializerOptions jsonOpt = new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = true };
        static public int Init(InitOptions opt)
        {



            var commands = String.Join(' ', opt.Commands);

            var config = new TaskConfig() { CreatedAt = DateTime.Now, Image = opt.Image, Execution = commands, Tasks = new List<string>() };
            if (File.Exists(opt.File))
            {
                var newName = $"wn_{JsonSerializer.Deserialize<TaskConfig>(File.ReadAllText(opt.File)).CreatedAt.ToString("o")}.json";
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"INFO: An existing configure file found, moving to '{newName}'");
                File.Move(opt.File, newName);
            }
            File.WriteAllText(opt.File, JsonSerializer.Serialize(config, jsonOpt));
            return 0;
        }
        static public int Add(AddOptions opt)
        {
            if (!File.Exists(defaultFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No such file '{defaultFile}'");
                return -1;
            }
            var content = JsonSerializer.Deserialize<TaskConfig>(File.ReadAllText(defaultFile));
            content.Tasks.Add(String.Join(' ', opt.Commands));
            File.WriteAllText(defaultFile, JsonSerializer.Serialize(content, jsonOpt));

            return 0;
        }
    }
}