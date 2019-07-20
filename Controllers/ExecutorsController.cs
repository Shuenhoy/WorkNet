using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WorkNet.Server.Models;

namespace WorkNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutorsController : ControllerBase
    {
        private readonly TaskContext context;
        public ExecutorsController(TaskContext c)
        {
            context = c;
        }
        // GET api/executors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Executor>>> Get()
        {
            return await context.Executors.ToListAsync();
        }

        // GET api/executors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Executor>> Get(int id)
        {
            var executor = await context.Executors.FindAsync(id);

            if (executor == null)
            {
                return NotFound();
            }
            return executor;
        }

        // POST api/executors
        [HttpPost]
        public async Task<ActionResult<Executor>> Post(Executor value)
        {
            context.Executors.Add(value);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { ExecutorId = value.ExecutorId }, value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Executor value)
        {
            if (id != value.ExecutorId)
            {
                return BadRequest();
            }

            context.Entry(value).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var executor = await context.Executors.FindAsync(id);

            if (executor == null)
            {
                return NotFound();
            }

            context.Executors.Remove(executor);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
