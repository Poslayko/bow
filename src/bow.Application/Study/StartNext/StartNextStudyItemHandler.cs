using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Study.StartNext;

public sealed class StartNextStudyItemHandler
{
    private readonly IUserRepository _users;
    private readonly IUserVocabularyProgressRepository _userProgresses;
    private readonly IVocabularyItemRepository _items;
    private readonly IUnitOfWork _unit;

    public StartNextStudyItemHandler(
        IUserRepository users,
        IUserVocabularyProgressRepository userProgresses,
        IVocabularyItemRepository items,
        IUnitOfWork unit
    )
    {
        _users = users;
        _userProgresses = userProgresses;
        _items = items;
        _unit = unit;
    }

    public async Task<StartNextStudyItemResult?> HandleAsync(
        StartNextStudyItemCommand query,
        CancellationToken cancellationToken
    )
    {
        var possibleUser = await _users.GetByIdAsync(query.UserId, cancellationToken);

        if (possibleUser is not {} user)
        {
            throw new NotFoundException("Wrong data");
        }

        if (user.LearningLanguage is not {} learningLanguage 
            || user.NativeLanguage is not {} nativeLanguage
            || user.LearningLevel is not {} learningLevel
        )
        {
            throw new ConflictException("LearningProfile is not configured.");
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