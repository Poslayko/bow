using bow.Application.Common.Interfaces;
using bow.Application.Telegram.Study.StartNext;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram.Study.SubmitAnswer;

public sealed class SubmitStudyAnswerHandler
{
    private readonly IUserRepository _users;
    private readonly IUserVocabularyProgressRepository _userProgresses;
    private readonly IVocabularyTranslationRepository _translations;
    private readonly IUnitOfWork _unit;
    private readonly ITelegramMessageSender _sender;
    private readonly StartNextStudyItemHandler _startNextStudyItemHandler;

    public SubmitStudyAnswerHandler(
        IUserRepository users, 
        IUserVocabularyProgressRepository userProgresses,
        IVocabularyTranslationRepository translations,
        IUnitOfWork unit,
        ITelegramMessageSender telegramMessageSender,
        StartNextStudyItemHandler startNextStudyItemHandler)
    {
        _users = users;
        _userProgresses = userProgresses;
        _translations = translations;
        _unit = unit;
        _sender = telegramMessageSender;
        _startNextStudyItemHandler = startNextStudyItemHandler;
    }

    public async Task HandleAsync(
        User user,
        TelegramAccount telegramAccount,
        string answer,
        long telegramId,
        CancellationToken cancellationToken
    )
    {
        if (user.LearningLanguage is not {} learningLanguage 
            || user.NativeLanguage is not {} nativeLanguage)
        {
            await _sender.SendTextMsg(
                telegramId, 
                "You need to set up your learning profile",
                cancellationToken);   

            return;
        }
        
        if (string.IsNullOrWhiteSpace(answer))
        {
            await _sender.SendTextMsg(
                telegramId, 
                "You need to answer, the studing item will be resend", 
                cancellationToken);

            await _startNextStudyItemHandler.HandleAsync(
                user,
                telegramAccount,
                telegramId,
                cancellationToken
            );
            
            return;
        }

        var now = DateTime.UtcNow;

        var waitingProgressItem = await _userProgresses.GetUserVocabularyProgressByStatusAsync(
            UserVocabularyProgressStatus.waiting_answer,
            user.Id,
            cancellationToken
        );

        if (waitingProgressItem is null)
        {
            await _sender.SendTextMsg(
                telegramId,
                "Something went wrong, waiting vocabulary item wasn't found",
                cancellationToken);

            return;
        }

        await ProcessProgressItem(
            waitingProgressItem,
            telegramId,
            nativeLanguage,
            answer,
            now,
            cancellationToken
        );
    }

    private async Task ProcessProgressItem(
        UserVocabularyProgress? progressItem,
        long telegramId,
        LanguageCode nativeLanguage,
        string answer,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        if (progressItem is null)
        {
            await _sender.SendTextMsg(
                telegramId,
                "We didn't find items that you were studing",
                cancellationToken
            );

            return;
        }

        var possibleEntityTranslations = await _translations.GetBySourceItemIdAndNativeLanguageAsync(
            progressItem.VocabularyItemId, 
            nativeLanguage, 
            cancellationToken
        );

        if (possibleEntityTranslations.Count == 0)
        {
            await _sender.SendTextMsg(
                telegramId,
                "Possible translations weren't found",
                cancellationToken);

            return;
        }

        var normalizedAnswer = VocabularyItem.NormalizeText(answer);

        bool isCorrect = false;
        List<string> possibleTranslations = new();

        foreach(var translation in possibleEntityTranslations)
        {
            possibleTranslations.Add(translation.TranslationTo.Text);
            if (translation.TranslationTo.NormalizedText == normalizedAnswer)
            {
                isCorrect = true;
            }
        }

        progressItem.ApplyAnswer(isCorrect, now);
        progressItem.SetStatus(UserVocabularyProgressStatus.idle);

        string responseText;
        
        if (isCorrect is true) {
            responseText = $"Your answer: {answer} was ✅\n possible translation: {string.Join(", ", possibleTranslations)}";
        } else
        {
            responseText = $"Your answer: {answer} was ❌\npossible translation: {string.Join(", ", possibleTranslations)}";
        }

        await _unit.SaveChangesAsync(cancellationToken);

        await _sender.SendTextMsg(
            telegramId,
            responseText,
            cancellationToken
        );        
    }
}