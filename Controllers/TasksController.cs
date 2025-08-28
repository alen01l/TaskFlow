using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {

        //Temporary static list to hold tasks in memory
        private static readonly List<TaskItem> _tasks = new()
        {
            new TaskItem { Title = "Hello TaskFlow", Priority = Priority.Medium, Status = Status.Backlog },
            new TaskItem { Title = "Second task", Priority = Priority.Low, Status = Status.InProgress }
        };


        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> Get()
            => Ok(_tasks.OrderByDescending(t => t.CreatedAt));

        public record CreateTaskDto(string Title);

        [HttpPost]
        public ActionResult<TaskItem> Create([FromBody] CreateTaskDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");
            var item = new TaskItem { Title = dto.Title.Trim() };
            _tasks.Add(item);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}
