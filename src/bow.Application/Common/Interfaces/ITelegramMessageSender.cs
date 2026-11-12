using bow.Domain.Enums;

namespace bow.Application.Common.Interfaces;

public interface ITelegramMessageSender
{
    Task SendWelcomeAsync(
        long telegramId,
        string? displayName,
        CancellationToken cancellationToken
    );

    Task SendNativeLanguageSelectionAsync(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task SendLearningLanguageSelectionAsync(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task SendErrorLearningAndNativeLanguageCanNotBeTheSame(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task SendLearningLevelSelectionAsync(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task ShowMenuHandlerAsync(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task ShowLearningMenuHandlerAsync(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task SendMsgUserLearningDataDoesNotSet(
        long telegramId,
        CancellationToken cancellationToken
    );

    Task SendTextMsg(
        long telegramId,
        string msg,
        CancellationToken cancellationToken
    );

    Task SendVocabularyItemQuestion(
        long telegramId,
        string text,
        CancellationToken cancellationToken
    );
}