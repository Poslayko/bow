using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Application.Study.StartNext;

namespace bow.Api.Endpoints.Study;

public static class StartNextStudyItemEndpoint
{
    public static IEndpointRouteBuilder MapPostNextStudyItemEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost(
            "/api/v1/study/next",
            HandleAsync
        );
    
        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        StartNextStudyItemRequest request,
        StartNextStudyItemHandler handler,
        ITelegramAccountRepository telegramAccountRepository,
        CancellationToken token
    )
    {
        var possibleUserId = await telegramAccountRepository.GetUserIdAsync(request.TelegramId, token);

        if (possibleUserId is not {} userId)
        {
            return Results.NotFound($"User with TelegramId: {request.TelegramId} wasn't found");
        }

        var query = new StartNextStudyItemCommand(userId);

        var result = await handler.HandleAsync(query, token);

        if (result is null)
        {
            return Results.NoContent();
        }

        var response = new StartNextStudyItemResponse(
            result.UserVocabularyProgressId,
            result.VocabularyItemId,
            result.Text,
            result.Language,
            result.Type,
            result.Stage,
            result.NextReviewAt,
            result.TargetLanguage
        );

        return Results.Ok(response);
    }
}
