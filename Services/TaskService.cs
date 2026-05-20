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

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(string userId, CancellationToken ct)
    {
        var items = await UserTasks(userId)
            .AsNoTracking()
            .ToListAsync(ct);

        return items
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public async Task<TaskItem?> GetTaskAsync(Guid id, string userId, CancellationToken ct)
    {
        return await UserTasks(userId)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<TaskItem> CreateTaskAsync(CreateTaskDto dto, string userId, CancellationToken ct)
    {
        var item = new TaskItem
        {
            Title = dto.Title.Trim(),
            UserId = userId
        };

        _dbContext.Tasks.Add(item);
        await _dbContext.SaveChangesAsync(ct);

        return item;
    }

    public async Task<TaskItem?> ReplaceTaskAsync(Guid id, ReplaceTaskDto dto, string userId, CancellationToken ct)
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
        item.DueAtUtc = dto.DueAtUtc;
        item.CompletedAt = dto.CompletedAt;

        await _dbContext.SaveChangesAsync(ct);

        return item;
    }

    public async Task<TaskItem?> UpdateTaskAsync(Guid id, UpdateTaskDto dto, string userId, CancellationToken ct)
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

        return item;
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