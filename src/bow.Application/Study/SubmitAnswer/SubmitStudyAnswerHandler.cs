using System.Reflection.Metadata;
using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Study.SubmitAnswer;

public sealed class SubmitStudyAnswerHandler
{
    private readonly IUserRepository _users;
    private readonly IUserVocabularyProgressRepository _userProgresses;
    private readonly IVocabularyTranslationRepository _translations;
    private readonly IUnitOfWork _unit;

    public SubmitStudyAnswerHandler(
        IUserRepository users, 
        IUserVocabularyProgressRepository userProgresses,
        IVocabularyTranslationRepository translations,
        IUnitOfWork unit)
    {
        _users = users;
        _userProgresses = userProgresses;
        _translations = translations;
        _unit = unit;
    }

    public async Task<SubmitStudyAnswerResult> HandleAsync(
        SubmitStudyAnswerCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(command.Answer))
        {
            throw new ArgumentException();
        }

        var user = await _users.GetByTelegramIdAsync(command.TelegramId);

        if (user is null)
        {
            throw new NotFoundException($"User with TelegramId: {command.TelegramId} was not found");
        }

        if (user.LearningLanguage is null || user.NativeLanguage is not { } nativeLanguage 
            || user.LearningLevel is null)
        {
            throw new ConflictException($"Learning profile was not set");
        }

        var progress = await _userProgresses.GetByIdAndUserIdAsync(command.UserVocabularyProgressId,
            user.Id, cancellationToken);

        if (progress is null)
        {
            throw new NotFoundException($"Progres for item: {command.UserVocabularyProgressId} was not found");
        }

        var previousStage = progress.Stage;

        var possibleEntityTranslations = await _translations.GetBySourceItemIdAndNativeLanguageAsync(
            progress.VocabularyItemId, nativeLanguage, cancellationToken
        );

        if (possibleEntityTranslations.Count == 0)
        {
            throw new ConflictException($"Possible translations weren't found");
        }

        var normalizedAnswer = VocabularyItem.NormalizeText(command.Answer);

        bool isCorrect = false;
        List<string> possibleTranslations = new();

        foreach(var translation in possibleEntityTranslations)
        {
            possibleTranslations.Add(translation.TranslationTo.NormalizedText);
            if (translation.TranslationTo.NormalizedText == normalizedAnswer)
            {
                isCorrect = true;
            }
        }

        var now = DateTime.UtcNow;

        progress.ApplyAnswer(isCorrect, now);

        await _unit.SaveChangesAsync(cancellationToken);

        return new SubmitStudyAnswerResult(progress.Id, isCorrect, previousStage, 
            progress.Stage, progress.LastReviewedAt, progress.NextReviewAt,
            possibleTranslations);
    }
}