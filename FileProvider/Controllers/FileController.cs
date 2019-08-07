using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkNet.FileProvider.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using LanguageExt;
using System.IO;
using System.Linq.Dynamic.Core;
using static LanguageExt.Prelude;

namespace WorkNet.FileProvider.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileEntryContext context;
        private readonly ReadonlyFileEntryContext readonlyContext;
        private readonly HttpClient client;
        public FileController(FileEntryContext c, ReadonlyFileEntryContext rc)
        {
            client = new HttpClient();
            readonlyContext = rc;
            context = c;

        }

        /// <summary>
        /// return the file by id.
        /// GET `api/file/@id/5`
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("@id/{id}")]
        public async Task<IActionResult> GetByID(int id)
        {
            var entry = await context.FileEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }
            var resp = await client.GetAsync($"http://volume:18080/{entry.SeaweedId}");
            var stream = await resp.Content.ReadAsStreamAsync();
            return File(stream, resp.Content.Headers.ContentType.MediaType, entry.FileName);

        }
        [HttpGet("@where/{cond}")]
        public async Task<ActionResult<List<FileEntry>>> GetByCondition(string cond)
        {
            return await readonlyContext.FileEntries
                .FromSql($"SELECT * FROM file_entries where " + cond)
                .ToListAsync();
        }
        [HttpGet("@idhead/{id}")]
        public async Task<ActionResult<FileEntry>> HeadByID(int id)
        {
            var entry = await context.FileEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }
            return entry;
        }
        [HttpGet("{namespace}:{name}")]
        public async Task<ActionResult<FileEntry>> GetByName(string _namespace, string name)
        {
            var entry = await context.FileEntries.Where(f => f.Namespace == _namespace && f.FileName == name).FirstAsync();
            if (entry == null)
            {
                return NotFound();
            }
            var resp = await client.GetAsync($"http://volume:18080/{entry.SeaweedId}");
            var stream = await resp.Content.ReadAsStreamAsync();
            return File(stream, resp.Content.Headers.ContentType.MediaType);
        }

        [HttpHead("{namespace}:{name}")]
        public async Task<ActionResult<FileEntry>> HeadByName(string _namespace, string name)
        {
            var entry = await context.FileEntries.Where(f => f.Namespace == _namespace && f.FileName == name).FirstAsync();
            if (entry == null)
            {
                return NotFound();
            }
            var resp = await client.GetAsync($"http://volume:18080/{entry.SeaweedId}");
            var stream = await resp.Content.ReadAsStreamAsync();
            return File(stream, resp.Content.Headers.ContentType.MediaType);
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult<FileEntry>> Post([FromForm] IFormFile files, [FromForm] string payload)
        {
            var entry = JsonSerializer.Deserialize<FileEntry>(payload);
            var assign = await client.GetAsync("http://master:9333/dir/assign?collection=file")
                .Bind(resp => resp.Content.ReadAsStringAsync())
                .Map(raw => JsonDocument.Parse(raw));


            entry.SeaweedId = assign.RootElement.GetProperty("fid").GetString();
            if (entry.Namespace is null)
            {
                return BadRequest("no `namespace provided`");
            }
            if (files is null)
            {
                files = Request.Form.Files.First();
            }
            if (entry.FileName is null)
            {
                entry.FileName = files.FileName;
            }
            entry.ExtName = Path.GetExtension(files.FileName);
            using var content = new MultipartFormDataContent();
            using var stream = files.OpenReadStream();
            entry.Size = (int)stream.Length;
            content.Add(new StreamContent(stream), "file");
            await client.PostAsync($"http://volume:18080/{entry.SeaweedId}", content);
            context.FileEntries.Add(entry);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(HeadByID), new { id = entry.FileEntryID }, entry);
        }


    }
}
