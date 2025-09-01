using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TasksController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> Get(CancellationToken ct)
        {
            var items = await _db.Tasks
                .AsNoTracking()
                .ToListAsync(ct);

            return Ok(items.OrderByDescending(t => t.CreatedAt));
        }

        public record CreateTaskDto(string Title);

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var item = new TaskItem { Title = dto.Title.Trim() };
            _db.Tasks.Add(item);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}
