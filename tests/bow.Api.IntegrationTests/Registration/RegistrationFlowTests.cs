using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using bow.Api.Endpoints.Users;
using bow.Api.IntegrationTests.Infrastructure;
using bow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Api.IntegrationTests.Registration;

public sealed class RegistrationFlowTests : IClassFixture<PostgresFixture>
{
    private readonly HttpClient _client;
    private readonly BowApiFactory _factory;

    public RegistrationFlowTests(PostgresFixture postgres)
    {
        _factory = new BowApiFactory(postgres.ConnectionString);
        _client = _factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new (JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter()}
    };

    [Fact]
    public async Task RegisterUser_WithTelegramId_GetUserId()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var telegramId = 1234567890;
        var response = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            TelegramId = telegramId
        });

        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(JsonOptions);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        Assert.NotNull(body);
        var user = await db.Users
            .Where(x => x.Id == body.UserId)
            .Include(x => x.TelegramAccount)
            .SingleOrDefaultAsync(CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal(body.UserId, user.Id);
        Assert.NotNull(user.TelegramAccount);
        Assert.True(user.TelegramAccount.Id > 0);
        Assert.Equal(telegramId, user.TelegramAccount.TelegramId);
    }

    [Fact]
    public async Task RegisterUser_SeveralTimes_GetSameUserForTelegramId()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var response = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            TelegramId = 1234567890
        });

        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(JsonOptions);

        var responseOnSecondTry = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            TelegramId = 1234567890
        });

        var bodyFromSecondTry = await responseOnSecondTry.Content
            .ReadFromJsonAsync<RegisterUserResponse>(JsonOptions);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        var totalUsers = await db.Users.CountAsync(CancellationToken.None);
        var totalTelegramUsers = await db.TelegramAccounts.CountAsync(CancellationToken.None);

        Assert.NotNull(body);
        Assert.NotNull(bodyFromSecondTry);
        Assert.Equal(body.UserId, bodyFromSecondTry.UserId);
        Assert.Equal(1, totalUsers);
        Assert.Equal(1, totalTelegramUsers);
    }
}