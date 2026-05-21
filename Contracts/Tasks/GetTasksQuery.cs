using TaskFlow.Api.Data;

namespace TaskFlow.Api.Contracts.Tasks;

public record GetTasksQuery(
    Status? Status,
    Priority? Priority,
    string? Search,
    string? Sort
);