namespace TaskFlow.Api.Contracts.Auth;

public record RegisterDto(
    string Email,
    string Password
);