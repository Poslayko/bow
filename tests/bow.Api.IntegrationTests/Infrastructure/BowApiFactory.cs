using bow.Application.Common.Interfaces;
using bow.Infrastructure.Persistence;
using bow.Infrastructure.Telegram;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace bow.Api.IntegrationTests.Infrastructure;

public sealed class BowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    public const string WebhookSecret = "test-webhook-secret";

    public FakeTelegramMessageSender TelegramSender { get; } = new();

    public BowApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseNpgsql(_connectionString)
                    .UseSnakeCaseNamingConvention());

            services.RemoveAll<ITelegramMessageSender>();
            services.AddSingleton<ITelegramMessageSender>(TelegramSender);

            var initializer = services.SingleOrDefault(x =>
                x.ServiceType == typeof(IHostedService)
                && x.ImplementationType == typeof(TelegramWebhookInitializer));

            if (initializer is not null)
            {
                services.Remove(initializer);
            }

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
        });

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Telegram:WebhookSecret"] = WebhookSecret
                }
            );
        });
    }
}