using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Data;
using TaskFlow.Api.Services.Tasks;

namespace TaskFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _tasks;
        private readonly UserManager<AppUser> _users;

        public TasksController(ITaskService tasks, UserManager<AppUser> users)
        {
            _tasks = tasks;
            _users = users;
        }

        private string UserId => _users.GetUserId(User)!;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> Get([FromQuery] GetTasksQuery query, CancellationToken ct)
        {
            var items = await _tasks.GetTasksAsync(query, UserId, ct);

            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskResponseDto>> Get(Guid id, CancellationToken ct)
        {
            var item = await _tasks.GetTaskAsync(id, UserId, ct);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {
            var item = await _tasks.CreateTaskAsync(dto, UserId, ct);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TaskResponseDto>> Put(Guid id, [FromBody] ReplaceTaskDto dto, CancellationToken ct)
        {
            var item = await _tasks.ReplaceTaskAsync(id, dto, UserId, ct);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<TaskResponseDto>> Patch(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
        {
            var item = await _tasks.UpdateTaskAsync(id, dto, UserId, ct);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _tasks.DeleteTaskAsync(id, UserId, ct);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}