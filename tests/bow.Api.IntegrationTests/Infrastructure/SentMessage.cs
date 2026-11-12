namespace bow.Api.IntegrationTests.Infrastructure;

public sealed record SentMessage(long TelegramId, string Text);