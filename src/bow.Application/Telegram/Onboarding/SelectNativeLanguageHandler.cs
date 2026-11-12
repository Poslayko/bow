using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram.Onboarding;

public sealed class SelectNativeLanguageHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public SelectNativeLanguageHandler(IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        User user,
        LanguageCode languageCode,
        CancellationToken cancellationToken
    )
    {
        user.SetNativeLanguage(languageCode);

        if(user.TelegramAccount is not {} telegramAccount)
        {
            return;
        }

        telegramAccount.SetState(TelegramUserState.ChoosingLearningLanguage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}