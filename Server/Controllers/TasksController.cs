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
using WorkNet.Common.Models;

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
        public GroupInfo GetGroutInfo(TaskGroup group)
        {
            var pulls = new HashSet<int>();
            var parameters = new List<JsonElement>();
            foreach (var item in group.SingleTasks)
            {
                parameters.Add(item._Parameters);
                foreach (var pull in item.Pulls)
                {
                    pulls.Add(pull);

                }
            }
            return new GroupInfo { Image = group.UserTask.Image, Execution = group.UserTask.Execution, Executor = group.UserTask.Executor, Parameters = parameters, Pulls = pulls.ToList(), Id = group.TaskGroupId };
        }
        [HttpGet("@group/{id}")]
        public async Task<ActionResult<GroupInfo>> GetGroup(int id)
        {
            var group = await context.TaskGroups.FindAsync(id);
            if (group == null) return NotFound();
            return GetGroutInfo(group);
        }
        [HttpPost("seterror/{id}")]
        public async Task<ActionResult<string>> UpdateFail(int id, [FromBody] string message)
        {
            var group = await context.TaskGroups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            group.ErrorMessage = message;
            group.Status = TaskGroupStatus.Error;
            group.UserTask.SubFinished++;
            await context.SaveChangesAsync();
            return "ok";
        }
        [HttpPost("result/{id}")]
        public async Task<ActionResult<string>> UpdateResult(int id, [FromBody] List<int> ids)
        {
            var group = await context.TaskGroups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            var i = 0;
            foreach (var single in group.SingleTasks)
            {
                single.Result = ids[i];
                i++;
            }
            group.Status = TaskGroupStatus.Success;
            group.UserTask.SubFinished++;
            await context.SaveChangesAsync();
            return "ok";
        }
        [HttpPost]
        public async Task<ActionResult<UserTask>> Submit([FromBody] TaskSubmit input)
        {

            var groups = taskDivider.Divide(input.tasks);
            var task = new UserTask()
            {
                SubFinished = 0,
                Image = input.executor.Image,
                Execution = input.executor.Execution,
                Executor = input.executor.OpExecutor,
                SubTotal = groups.Count(),
                SubTasks = groups,
                SubmitTime = DateTime.Now
            };
            context.UserTasks.Add(task);
            await context.SaveChangesAsync();

            foreach (var group in task.SubTasks)
            {
                rabbitMQ.Publish(group.TaskGroupId);
            }
            return CreatedAtAction(nameof(GetByID), new { id = task.UserTaskId }, task);
        }
    }
}