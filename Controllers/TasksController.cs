using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WorkNet.Server.Models;
using WorkNet.Server.Services;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WorkNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ServerContext context;
        private readonly ITaskDivisionService taskDivider;
        private readonly RabbitMQService rabbitMQ;
        public TasksController(ServerContext c, ITaskDivisionService t, RabbitMQService r)
        {
            context = c;
            taskDivider = t;
            rabbitMQ = r;
        }
        [HttpGet("")]
        public async Task<ActionResult<List<UserTask>>> Get()
        {
            return await context.UserTasks.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTask>> GetByID(int id)
        {
            var task = await context.UserTasks.FindAsync(id);
            if (task == null) return NotFound();
            return task;
        }
        public class TaskSubmit
        {
            public Executor executor { get; set; }
            public List<SingleTask> tasks { get; set; }
        }
        public class GroupInfo
        {

            public List<JsonElement> Parameters { get; set; }
            public List<string> Pulls { get; set; }
        }
        [HttpGet("@group/{id}")]
        public async Task<ActionResult<GroupInfo>> GetGroup(int id)
        {
            var group = await context.TaskGroups.FindAsync(id);
            if (group == null) return NotFound();
            var pulls = new HashSet<string>();
            var parameters = new List<JsonElement>();
            foreach (var item in group.SingleTasks)
            {
                parameters.Add(item._Parameters);
                foreach (var pull in item.Pulls)
                {
                    pulls.Add(pull);

                }
            }
            Console.WriteLine((parameters, pulls.ToList()));
            return new GroupInfo { Parameters = parameters, Pulls = pulls.ToList() };
        }

        [HttpPost]
        public async Task<ActionResult<UserTask>> Submit([FromBody] TaskSubmit input)
        {
            var groups = taskDivider.Divide(input.tasks);
            var task = new UserTask()
            {
                SubFinished = 0,
                executor = input.executor,
                SubTotal = groups.Count(),
                SubTasks = groups,
                SubmitTime = DateTime.Now
            };
            context.UserTasks.Add(task);
            foreach (var group in task.SubTasks)
            {
                rabbitMQ.Publish(group);
            }
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByID), new { id = task.UserTaskId }, task);
        }
    }
}