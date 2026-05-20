using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Services.Tasks;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponseDto>> GetTasksAsync(string userId, CancellationToken ct);
    Task<TaskResponseDto?> GetTaskAsync(Guid id, string userId, CancellationToken ct);
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, string userId, CancellationToken ct);
    Task<TaskResponseDto?> ReplaceTaskAsync(Guid id, ReplaceTaskDto dto, string userId, CancellationToken ct);
    Task<TaskResponseDto?> UpdateTaskAsync(Guid id, UpdateTaskDto dto, string userId, CancellationToken ct);
    Task<bool> DeleteTaskAsync(Guid id, string userId, CancellationToken ct);
}