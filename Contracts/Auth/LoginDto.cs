namespace TaskFlow.Api.Contracts.Auth;

public record LoginDto(
    string Email,
    string Password
);