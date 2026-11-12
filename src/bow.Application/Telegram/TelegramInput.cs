namespace bow.Application.Telegram;

public sealed record TelegramInput(
    long TelegramUserId,
    long ChatId,
    string? UserName,
    string? Text,
    string? CallbackData
);