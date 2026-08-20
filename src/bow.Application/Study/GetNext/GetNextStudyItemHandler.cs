using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;

namespace bow.Application.Study.GetNext;

public sealed class GetNextStudyItemHandler
{
    private readonly IUserRepository _user;
    private readonly IUserVocabularyProgressRepository _userProgress;

    public GetNextStudyItemHandler(
        IUserRepository user,
        IUserVocabularyProgressRepository userProgress
    )
    {
        _user = user;
        _userProgress = userProgress;
    }

    public async Task<GetNextStudyItemResult?> HandleAsync(
        GetNextStudyItemQuery query,
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
            || user.LearningLevel is not {} 
        )
        {
            throw new ConflictException("LearningProfile is not configured.");
        }

        var nextItem = await _userProgress.GetNextDueAsync(user.Id, learningLanguage,
            nativeLanguage, DateTime.UtcNow, cancellationToken);
        
        if (nextItem is null)
        {
            return null;
        }

        return new GetNextStudyItemResult(
            nextItem.Id,
            nextItem.VocabularyItemId,
            nextItem.VocabularyItem.Text,
            learningLanguage,
            nextItem.VocabularyItem.Type,
            nextItem.Stage,
            nextItem.NextReviewAt,
            nativeLanguage
        );
    }
}