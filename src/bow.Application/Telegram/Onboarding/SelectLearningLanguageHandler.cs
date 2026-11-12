using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram.Onboarding;

public sealed class SelectLearningLanguageHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public SelectLearningLanguageHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        User user,
        string language,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<LanguageCode>(language, ignoreCase: true, out var languageCode)
            || !Enum.IsDefined(languageCode))
        {
            return;
        }

        user.SetLearningLanguage(languageCode);

        if(user.TelegramAccount is not {} telegramAccount)
        {
            return;
        }

        telegramAccount.SetState(TelegramUserState.ChoosingLearningLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}