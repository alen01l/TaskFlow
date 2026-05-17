namespace TaskFlow.Api.Contracts.Tasks
{
    public record ReplaceTaskDto(
        string Title,
        Priority Priority,
        Status Status,
        DateTimeOffset? DueAtUtc,
        DateTimeOffset? CompletedAt
    );
}
