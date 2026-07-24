namespace bow.Api.Endpoints.Users;

public sealed record RegisterUserRequest(
    long TelegramId,
    string? DisplayName
);