namespace TaskFlow.Api.Contracts.Tasks
{
    public record ReplaceTaskDto(
    string Title,
    string? Description,
    Priority Priority,
    Status Status,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset? CompletedAt
);
}
