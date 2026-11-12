using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Telegram.Onboarding;

public sealed class ShowMenuHandler
{
    private readonly ITelegramMessageSender _sender;
    private readonly IUnitOfWork _unitOfWork;

    public ShowMenuHandler(ITelegramMessageSender sender,
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
        await _sender.ShowMenuHandlerAsync(telegramId, cancellationToken);

        telegramAccount.SetState(Domain.Enums.TelegramUserState.ChoosingInMenu);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}