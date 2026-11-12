using System.Text.Json;
using System.Text.Json.Serialization;
using bow.Api.IntegrationTests.Infrastructure;

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

    
}