using Microsoft.AspNetCore.Authorization;
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

        private string UserId => _users.GetUserId(User)!;

        private IQueryable<TaskItem> UserTasks =>
            _dbContext.Tasks.Where(t => t.UserId == UserId);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> Get(CancellationToken ct)
        {
            var items = await UserTasks
                .AsNoTracking()
                .ToListAsync(ct);

            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskItem>> Get(Guid id, CancellationToken ct)
        {
            var item = await UserTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {
            var item = new TaskItem
            {
                Title = dto.Title.Trim(),
                UserId = UserId
            };

            _dbContext.Tasks.Add(item);
            await _dbContext.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TaskItem>> Put(Guid id, [FromBody] ReplaceTaskDto dto, CancellationToken ct)
        {
            var item = await UserTasks
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (item is null)
            {
                return NotFound();
            }

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
            var item = await UserTasks
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (item is null)
            {
                return NotFound();
            }

            if (dto.Title is not null)
            {
                item.Title = dto.Title.Trim();
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
            {
                item.DueAtUtc = dto.DueAtUtc;
            }

            if (dto.MarkComplete.HasValue)
            {
                item.CompletedAt = dto.MarkComplete.Value
                    ? DateTimeOffset.UtcNow
                    : null;
            }

            await _dbContext.SaveChangesAsync(ct);

            return Ok(item);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var item = await UserTasks
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (item is null)
            {
                return NotFound();
            }

            _dbContext.Tasks.Remove(item);
            await _dbContext.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}