using bow.Application.Users.ConfigureLearning;

namespace bow.Api.Endpoints.Users;

public static class ConfigureLearningUserEndpoint
{
    public static IEndpointRouteBuilder MapConfigureLearningUserEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPut(
            "/api/v1/users/{telegramId}/learning-profile",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        long telegramId,
        ConfigureLearningUserRequest request,
        ConfigureLearningUserHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new ConfigureLearningUserCommand(
            telegramId, 
            request.NativeLanguage,
            request.LearningLanguage,
            request.LearningLevel
        );

        await handler.HandleAsync(command, cancellationToken);

        return Results.NoContent();
    }
}