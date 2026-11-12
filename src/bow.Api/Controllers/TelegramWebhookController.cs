using bow.Application.Telegram;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace bow.Api.Controllers;

[ApiController]
[Route("/api/v1/telegram/webhook")]
public sealed class TelegramWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public TelegramWebhookController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken,
        HandleTelegramInputHandler handler,
        CancellationToken cancellationToken
    )
    {
        var expectedSecret = _configuration["Telegram:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(expectedSecret) || secretToken != expectedSecret)
        {
            return Unauthorized();
        }

        var telegramId = update.CallbackQuery?.From.Id 
            ?? update?.Message?.From?.Id;
        var chatId = update?.CallbackQuery?.Message?.Chat.Id 
            ?? update?.Message?.Chat?.Id;
        var userName = update?.CallbackQuery?.From.Username 
            ?? update?.Message?.From?.Username;
        var text = update?.Message?.Text;
        var callbackData = update?.CallbackQuery?.Data;
        var chatType = update?.CallbackQuery?.Message?.Chat?.Type
            ?? update?.Message?.Chat.Type;

        if (telegramId is not {} realTelegramId 
            || chatId is not {} realChatId
            || chatType != ChatType.Private)
        {
            return NoContent();
        }

        var input = new TelegramInput(realTelegramId, realChatId, userName, text, callbackData);

        await handler.HandleAsync(input, cancellationToken);

        return NoContent();
    }
}