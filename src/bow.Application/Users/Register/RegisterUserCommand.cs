namespace bow.Application.Users.Register;

public sealed record RegisterUserCommand(
    long TelegramId,
    string? DisplayName
);