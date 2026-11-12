using bow.Application.Common.Interfaces;
using bow.Application.Telegram.Onboarding;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram.Study.StartNext;

public sealed class StartNextStudyItemHandler
{
    private readonly IUserRepository _users;
    private readonly IUserVocabularyProgressRepository _userProgresses;
    private readonly IVocabularyItemRepository _items;
    private readonly IUnitOfWork _unit;
    private readonly ITelegramMessageSender _sender;
    private readonly ShowMenuHandler _showMenuHandler;

    public StartNextStudyItemHandler(
        IUserRepository users,
        IUserVocabularyProgressRepository userProgresses,
        IVocabularyItemRepository items,
        IUnitOfWork unit,
        ITelegramMessageSender sender,
        ShowMenuHandler showMenuHandler
    )
    {
        _users = users;
        _userProgresses = userProgresses;
        _items = items;
        _unit = unit;
        _sender = sender;
        _showMenuHandler = showMenuHandler;
    }

    public async Task HandleAsync(
        User user,
        TelegramAccount telegramAccount,
        long telegramId,
        CancellationToken cancellationToken
    )
    {
        if (user.LearningLanguage is not {} learningLanguage 
            || user.NativeLanguage is not {} nativeLanguage
            || user.LearningLevel is not {} learningLevel
        )
        {
            await _sender.SendMsgUserLearningDataDoesNotSet(telegramId, cancellationToken);

            return;
        }

        var waitingProgress = await _userProgresses.GetUserVocabularyProgressByStatusAsync(
            UserVocabularyProgressStatus.waiting_answer, user.Id, cancellationToken
        );

        if (waitingProgress is not null)
        {
            await SendVocabularyItemAndChangeState(
                telegramAccount,
                telegramId,
                waitingProgress,
                cancellationToken
            );

            return;
        }

        var now = DateTime.UtcNow;

        var nextItem = await _userProgresses.GetNextDueAsync(user.Id, learningLanguage,
            nativeLanguage, now, cancellationToken);

        UserVocabularyProgress? possibleNextItem = nextItem is null
            ? await CreateProgressIfPossibleAsync(
                user.Id,
                learningLevel,
                learningLanguage,
                nativeLanguage,
                now,
                cancellationToken
            ) : null;
        
        var studyItem = nextItem ?? possibleNextItem;

        if (studyItem is null)
        {
            await _sender.SendTextMsg(telegramId, "You don't have new words for learning",
                cancellationToken);

            await _showMenuHandler.HandleAsync(telegramId, telegramAccount, cancellationToken);

            return;
        }

        await SendVocabularyItemAndChangeState(
            telegramAccount,
            telegramId,
            studyItem,
            cancellationToken);
    }

    private async Task SendVocabularyItemAndChangeState(
        TelegramAccount telegramAccount,
        long telegramId,
        UserVocabularyProgress studyItem,
        CancellationToken cancellationToken
    )
    {
        telegramAccount.SetState(TelegramUserState.SubmitAnswer);
        studyItem.SetStatus(UserVocabularyProgressStatus.waiting_answer);

        await _unit.SaveChangesAsync(cancellationToken);

        await _sender.SendVocabularyItemQuestion(
            telegramId, 
            studyItem.VocabularyItem.Text,
            cancellationToken);
    }

    private async Task<UserVocabularyProgress?> CreateProgressIfPossibleAsync(
        int userId, 
        CefrLevel level,
        LanguageCode learningLanguage,
        LanguageCode nativeLanguage,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var allowedLevels = UserVocabularyProgress.GetAllowedLevels(level);
        var possibleNextItemId = await _items.TryToGetPossibleNextItemIdAsync(userId,
            allowedLevels, learningLanguage, nativeLanguage, cancellationToken);
        
        if (possibleNextItemId is not {} existingPossibleNextItemId)
        {
            return null;
        }  

        var possibleNextProgress = new UserVocabularyProgress(userId, existingPossibleNextItemId, 
            now);

        await _userProgresses.AddAsync(possibleNextProgress, cancellationToken);

        await _unit.SaveChangesAsync(cancellationToken);

        return await _userProgresses.GetByIdAndUserIdAsync(
            possibleNextProgress.Id,
            userId,
            cancellationToken
        );
    }
}