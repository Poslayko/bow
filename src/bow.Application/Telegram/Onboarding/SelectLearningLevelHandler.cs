using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram.Onboarding;

public sealed class SelectLearningLevelHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public SelectLearningLevelHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        User user,
        CefrLevel cefrLevel,
        CancellationToken cancellationToken
    )
    {
        user.SetLearningLevel(cefrLevel);

        if(user.TelegramAccount is not {} telegramAccount)
        {
            return;
        }

        telegramAccount.SetState(TelegramUserState.ChoosingInMenu);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}