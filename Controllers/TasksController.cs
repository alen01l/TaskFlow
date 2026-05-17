using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts.Tasks;
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



        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {

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

            item.Priority = dto.Priority;
            item.Status = dto.Status;

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title cannot be empty.");

            // Replace all fields
            item.Title = dto.Title.Trim();
            item.Priority = dto.Priority;
            item.Status = dto.Status;
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

            if (dto.Priority.HasValue)
            {
                item.Priority = dto.Priority.Value;
            }

            if (dto.Status.HasValue)
            {
                item.Status = dto.Status.Value;
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
