using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Telegram.Onboarding;

public sealed class ChooseNativeLanguageHandler
{
    private readonly ITelegramMessageSender _sender;
    private readonly IUnitOfWork _unit;

    public ChooseNativeLanguageHandler(ITelegramMessageSender sender, IUnitOfWork unit)
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
        await _sender.SendNativeLanguageSelectionAsync(telegramId, cancellationToken);

        telegramAccount.SetState(Domain.Enums.TelegramUserState.ChoosingNativeLanguage);

        await _unit.SaveChangesAsync(cancellationToken);
    }
}