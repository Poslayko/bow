using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Study.StartNext;

public sealed class StartNextStudyItemHandler
{
    private readonly IUserRepository _user;
    private readonly IUserVocabularyProgressRepository _userProgress;
    private readonly IVocabularyItemRepository _item;
    private readonly IUnitOfWork _unit;

    public StartNextStudyItemHandler(
        IUserRepository user,
        IUserVocabularyProgressRepository userProgress,
        IVocabularyItemRepository item,
        IUnitOfWork unit
    )
    {
        _user = user;
        _userProgress = userProgress;
        _item = item;
        _unit = unit;
    }

    public async Task<StartNextStudyItemResult?> HandleAsync(
        StartNextStudyItemCommand query,
        CancellationToken cancellationToken
    )
    {
        var user = await _user.GetByTelegramIdAsync(query.TelegramId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with TelegramId: {query.TelegramId} wasn't found");
        }

        if (user.LearningLanguage is not {} learningLanguage 
            || user.NativeLanguage is not {} nativeLanguage
            || user.LearningLevel is not {} learningLevel
        )
        {
            throw new ConflictException("LearningProfile is not configured.");
        }

        var now = DateTime.UtcNow;

        var nextItem = await _userProgress.GetNextDueAsync(user.Id, learningLanguage,
            nativeLanguage, now, cancellationToken);
        
        UserVocabularyProgress? possibleNextItem = nextItem is null
            ? await TryToGetPossibleNextItemAsync(
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
            return null;
        }

        return new StartNextStudyItemResult(
            studyItem.Id,
            studyItem.VocabularyItemId,
            studyItem.VocabularyItem.Text,
            learningLanguage,
            studyItem.VocabularyItem.Type,
            studyItem.Stage,
            studyItem.NextReviewAt,
            nativeLanguage
        );
    }

    private async Task<UserVocabularyProgress?> TryToGetPossibleNextItemAsync(
        int userId, 
        CefrLevel level,
        LanguageCode learningLanguage,
        LanguageCode nativeLanguage,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var allowedLevels = UserVocabularyProgress.GetAllowedLevels(level);
        var possibleNextItemId = await _item.TryToGetPossibleNextItemIdAsync(userId,
            allowedLevels, learningLanguage, nativeLanguage, cancellationToken);
            
        if (possibleNextItemId is not {} existingPossibleNextItemId)
        {
            return null;
        }  

        var possibleNextProgress = new UserVocabularyProgress(userId, existingPossibleNextItemId, 
            now);

        await _userProgress.AddAsync(possibleNextProgress, cancellationToken);

        await _unit.SaveChangesAsync(cancellationToken);

        return await _userProgress.GetByIdAndUserIdAsync(
            possibleNextProgress.Id,
            userId,
            cancellationToken
        );
    }
}