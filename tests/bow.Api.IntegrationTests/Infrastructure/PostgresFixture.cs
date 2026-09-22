using Testcontainers.PostgreSql;

namespace bow.Api.IntegrationTests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("bow")
        .WithUsername("bow")
        .WithPassword("small_password")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }
}