using bow.Application.Common.Interfaces;

namespace bow.Application.Users.ConfigureLearning;

public sealed class SendPossibleNativeLanguageHandler
{
    private readonly ITelegramMessageSender _sender;

    public SendPossibleNativeLanguageHandler(ITelegramMessageSender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(
        SendPossibleNativeLanguageCommand command,
        CancellationToken token
    )
    {
        await _sender.SendNativeLanguageSelectionAsync(command.telegramId, token);
    }
}