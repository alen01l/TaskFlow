using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _users;
        public TasksController(AppDbContext dbContext, UserManager<AppUser> users)
        {
            _dbContext = dbContext;
            _users = users;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> Get(CancellationToken ct)
        {
            var userId = _users.GetUserId(User)!;

            var items = await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .ToListAsync(ct);

            return Ok(items.OrderByDescending(t => t.CreatedAt));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskItem>> Get(Guid id, CancellationToken ct)
        {
            var userId = _users.GetUserId(User)!;

            var item = await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }



        public record CreateTaskDto(string Title);

        public record UpdateTaskDto(
            string? Title,
            string? Priority,   // "Low" | "Medium" | "High" 
            string? Status,     // "Backlog" | "InProgress" | "Done" 
            DateTimeOffset? DueAtUtc,
            bool? MarkComplete  // true = set CompletedAt now, false = clear CompletedAt
        );

        public record ReplaceTaskDto(
            string Title,
            string Priority,    // "Low" | "Medium" | "High"
            string Status,      // "Backlog" | "InProgress" | "Done"
            DateTimeOffset? DueAtUtc,
            DateTimeOffset? CompletedAt
        );



        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var userId = _users.GetUserId(User)!;

            var item = new TaskItem { Title = dto.Title.Trim(), UserId = userId };
            _dbContext.Tasks.Add(item);
            await _dbContext.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TaskItem>> Put(Guid id, [FromBody] ReplaceTaskDto dto, CancellationToken ct)
        {
            var userId = _users.GetUserId(User)!;

            var item = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

            if (item is null) return NotFound();

            // Validate Priority
            if (!Enum.TryParse<Priority>(dto.Priority, ignoreCase: true, out var prio))
                return BadRequest("Priority must be Low, Medium, or High.");

            // Validate Status
            if (!Enum.TryParse<Status>(dto.Status, ignoreCase: true, out var status))
                return BadRequest("Status must be Backlog, InProgress, or Done.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title cannot be empty.");

            // Replace all fields
            item.Title = dto.Title.Trim();
            item.Priority = prio;
            item.Status = status;
            item.DueAtUtc = dto.DueAtUtc;
            item.CompletedAt = dto.CompletedAt;

            await _dbContext.SaveChangesAsync(ct);
            return Ok(item);
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<TaskItem>> Patch(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
        {
            var userId = _users.GetUserId(User)!;
            var item = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
            if (item is null)
            {
                return NotFound();
            }

            if (dto.Title is not null)
            {
                var title = dto.Title.Trim();
                if (string.IsNullOrWhiteSpace(title)) return BadRequest("Title cannot be empty.");
                item.Title = title;
            }

            if (dto.Priority is not null)
            {
                if (!Enum.TryParse<Priority>(dto.Priority, ignoreCase: true, out var prio))
                    return BadRequest("Priority must be Low, Medium, or High.");
                item.Priority = prio;
            }

            if (dto.Status is not null)
            {
                if (!Enum.TryParse<Status>(dto.Status, ignoreCase: true, out var st))
                    return BadRequest("Status must be Backlog, InProgress, or Done.");
                item.Status = st;
            }

            if (dto.DueAtUtc.HasValue)
                item.DueAtUtc = dto.DueAtUtc;

            if (dto.MarkComplete.HasValue)
                item.CompletedAt = dto.MarkComplete.Value ? DateTimeOffset.UtcNow : null;

            await _dbContext.SaveChangesAsync(ct);
            return Ok(item);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userId = _users.GetUserId(User)!;

            var item = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

            if (item is null) return NotFound();

            _dbContext.Tasks.Remove(item);
            await _dbContext.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
