using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Telegram.Onboarding;

public sealed class ShowLearningMenuHandler
{
    private readonly ITelegramMessageSender _sender;
    private readonly IUnitOfWork _unit;

    public ShowLearningMenuHandler(ITelegramMessageSender sender, IUnitOfWork unit)
    {
        _sender = sender;
        _unit = unit;
    }

    public async Task HandleAsync(
        long telegramId,
        TelegramAccount telegramAccount,
        CancellationToken cancellationToken
    )
    {
        await _sender.ShowLearningMenuHandlerAsync(telegramId, cancellationToken);

        telegramAccount.SetState(Domain.Enums.TelegramUserState.InLearningMenu);

        await _unit.SaveChangesAsync(cancellationToken);
    }
}