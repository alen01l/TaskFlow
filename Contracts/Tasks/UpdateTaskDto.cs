namespace TaskFlow.Api.Contracts.Tasks;

public record UpdateTaskDto(
    string? Title,
    string? Description,
    Priority? Priority,
    Status? Status,
    DateTimeOffset? DueAtUtc,
    bool? MarkComplete
);