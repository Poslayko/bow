using bow.Application.Common.Interfaces;

namespace bow.Api.IntegrationTests.Infrastructure;

public sealed class FakeTelegramMessageSender : ITelegramMessageSender
{
    private readonly List<SentMessage> _messages = [];
    public IReadOnlyList<SentMessage> Messages => _messages;

    public Task SendErrorLearningAndNativeLanguageCanNotBeTheSame(long telegramId, 
        CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "SendErrorLearningAndNativeLanguageCanNotBeTheSame"));
        return Task.CompletedTask;
    }

    public Task SendLearningLanguageSelectionAsync(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "SendLearningLanguageSelectionAsync"));
        return Task.CompletedTask;
    }

    public Task SendLearningLevelSelectionAsync(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "SendLearningLevelSelectionAsync"));
        return Task.CompletedTask;
    }

    public Task SendMsgUserLearningDataDoesNotSet(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "SendMsgUserLearningDataDoesNotSet"));
        return Task.CompletedTask;
    }

    public Task SendNativeLanguageSelectionAsync(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "SendNativeLanguageSelectionAsync"));
        return Task.CompletedTask;
    }

    public Task SendTextMsg(long telegramId, string msg, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, msg));
        return Task.CompletedTask;
    }

    public Task SendVocabularyItemQuestion(long telegramId, string text, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, text));
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(long telegramId, string? displayName, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, $"SendWelcomeAsync:{displayName}"));
        return Task.CompletedTask;
    }

    public Task ShowLearningMenuHandlerAsync(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "ShowLearningMenuHandlerAsync"));
        return Task.CompletedTask;
    }

    public Task ShowMenuHandlerAsync(long telegramId, CancellationToken cancellationToken)
    {
        _messages.Add(new SentMessage(telegramId, "ShowMenuHandlerAsync"));
        return Task.CompletedTask;
    }
}