using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace bow.Infrastructure.Telegram;

public sealed class TelegramWebhookInitializer : IHostedService
{
    private readonly ITelegramBotClient _client;
    private readonly IConfiguration _configuration;

    public TelegramWebhookInitializer(
        ITelegramBotClient client,
        IConfiguration configuration
    )
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancelationToken)
    {
        var webhookUrl = _configuration["Telegram:WebhookUrl"];

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return;
        }

        await SetWebhookIfItNeededAsync(webhookUrl, cancelationToken);
    }

    private async Task SetWebhookIfItNeededAsync(
        String webhookUrl,
        CancellationToken cancellationToken
    )
    {
        var response = await _client.GetWebhookInfo(cancellationToken: cancellationToken);

        if (response.Url != webhookUrl)
        {
            await _client.SetWebhook(
                url: webhookUrl,
                secretToken: _configuration["Telegram:WebhookSecret"],
                cancellationToken: cancellationToken
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}