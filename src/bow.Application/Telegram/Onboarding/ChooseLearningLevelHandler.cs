using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Telegram.Onboarding;

public sealed class ChooseLearningLevelHandler
{
    private readonly ITelegramMessageSender _sender;
    private readonly IUnitOfWork _unitOfWork;

    public ChooseLearningLevelHandler(ITelegramMessageSender sender,
        IUnitOfWork unitOfWork)
    {
        _sender = sender;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        long telegramId,
        TelegramAccount telegramAccount,
        CancellationToken cancellationToken
    )
    {
        await _sender.SendLearningLevelSelectionAsync(telegramId, cancellationToken);

        telegramAccount.SetState(Domain.Enums.TelegramUserState.ChoosingLearningLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}