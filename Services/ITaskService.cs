using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Services.Tasks;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(string userId, CancellationToken ct);
    Task<TaskItem?> GetTaskAsync(Guid id, string userId, CancellationToken ct);
    Task<TaskItem> CreateTaskAsync(CreateTaskDto dto, string userId, CancellationToken ct);
    Task<TaskItem?> ReplaceTaskAsync(Guid id, ReplaceTaskDto dto, string userId, CancellationToken ct);
    Task<TaskItem?> UpdateTaskAsync(Guid id, UpdateTaskDto dto, string userId, CancellationToken ct);
    Task<bool> DeleteTaskAsync(Guid id, string userId, CancellationToken ct);
}