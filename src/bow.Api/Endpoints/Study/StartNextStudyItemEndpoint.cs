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
        CancellationToken cancellationToken
    )
    {
        var query = new StartNextStudyItemCommand(request.TelegramId);

        var result = await handler.HandleAsync(query, cancellationToken);

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
