namespace TaskFlow.Api.Contracts.Tasks;

public record CreateTaskDto(
    string Title,
    string? Description
);