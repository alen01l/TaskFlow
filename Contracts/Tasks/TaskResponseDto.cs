using TaskFlow.Api.Data;

namespace TaskFlow.Api.Contracts.Tasks;

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    Status Status,
    Priority Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset? CompletedAt
)
{
    public static TaskResponseDto FromTask(TaskItem task)
    {
        return new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.CreatedAt,
            task.DueAtUtc,
            task.CompletedAt
        );
    }
}