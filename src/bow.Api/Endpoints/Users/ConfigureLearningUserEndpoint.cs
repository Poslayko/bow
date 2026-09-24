using bow.Application.Common.Interfaces;
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
        ITelegramAccountRepository telegramAccountRepository,
        CancellationToken token
    )
    {
        var possibleUserId = await telegramAccountRepository.GetUserIdAsync(telegramId, token);

        if (possibleUserId is not {} userId)
        {
            return Results.NotFound($"User with TelegramId: {telegramId} wasn't found");
        }

        var command = new ConfigureLearningUserCommand(
            userId, 
            request.NativeLanguage,
            request.LearningLanguage,
            request.LearningLevel
        );

        await handler.HandleAsync(command, token);

        return Results.NoContent();
    }
}