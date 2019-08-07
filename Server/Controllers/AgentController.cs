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
    public class AgentController : ControllerBase
    {
        private readonly ServerContext context;
        public AgentController(ServerContext c)
        {
            context = c;
        }
    }
}