using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private IQueryable<TaskItem> UserTasks(string userId) =>
        _dbContext.Tasks.Where(t => t.UserId == userId);

    public async Task<IReadOnlyList<TaskResponseDto>> GetTasksAsync(
    GetTasksQuery query,
    string userId,
    CancellationToken ct
)
    {
        var tasks = UserTasks(userId).AsNoTracking();

        if (query.Status.HasValue)
        {
            tasks = tasks.Where(t => t.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            tasks = tasks.Where(t => t.Priority == query.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();

            tasks = tasks.Where(t => t.Title.ToLower().Contains(search));
        }

        var items = await tasks.ToListAsync(ct);

        var sorted = query.Sort?.ToLowerInvariant() switch
        {
            "oldest" => items.OrderBy(t => t.CreatedAt),
            "priority" => items.OrderByDescending(t => t.Priority),
            "status" => items.OrderBy(t => t.Status),
            _ => items.OrderByDescending(t => t.CreatedAt),
        };

        return sorted
            .Select(TaskResponseDto.FromTask)
            .ToList();
    }

    public async Task<TaskResponseDto?> GetTaskAsync(Guid id, string userId, CancellationToken ct)
    {
        var item = await UserTasks(userId)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return item is null
            ? null
            : TaskResponseDto.FromTask(item);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, string userId, CancellationToken ct)
    {
        var item = new TaskItem
        {
            Title = dto.Title.Trim(),
            UserId = userId,
            Description = dto.Description?.Trim()
        };

        _dbContext.Tasks.Add(item);
        await _dbContext.SaveChangesAsync(ct);

        return TaskResponseDto.FromTask(item);
    }

    public async Task<TaskResponseDto?> ReplaceTaskAsync(Guid id, ReplaceTaskDto dto, string userId, CancellationToken ct)
    {
        var item = await UserTasks(userId)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (item is null)
        {
            return null;
        }

        item.Title = dto.Title.Trim();
        item.Priority = dto.Priority;
        item.Status = dto.Status;
        item.Description = dto.Description?.Trim();
        item.DueAtUtc = dto.DueAtUtc;
        item.CompletedAt = dto.CompletedAt;

        await _dbContext.SaveChangesAsync(ct);

        return TaskResponseDto.FromTask(item);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(Guid id, UpdateTaskDto dto, string userId, CancellationToken ct)
    {
        var item = await UserTasks(userId)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (item is null)
        {
            return null;
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

        if (dto.Description is not null)
        {
            item.Description = dto.Description.Trim();
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

        return TaskResponseDto.FromTask(item);
    }

    public async Task<bool> DeleteTaskAsync(Guid id, string userId, CancellationToken ct)
    {
        var item = await UserTasks(userId)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (item is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(item);
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }
}