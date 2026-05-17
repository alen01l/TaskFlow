namespace TaskFlow.Api.Contracts.Tasks;

public record UpdateTaskDto(
    string? Title,
    Priority? Priority,
    Status? Status,
    DateTimeOffset? DueAtUtc,
    bool? MarkComplete
);