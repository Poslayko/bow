using bow.Application.Study.GetNext;
using Microsoft.AspNetCore.Mvc;

namespace bow.Api.Endpoints.Study;

public static class GetNextStudyItemEndpoint
{
    public static IEndpointRouteBuilder MapGetNextStudyItemEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapGet(
            "/api/v1/study/next",
            HandleAsync
        );
    
        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetNextStudyItemRequest request,
        [FromServices] GetNextStudyItemHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetNextStudyItemQuery(request.TelegramId);

        var result = await handler.HandleAsync(query, cancellationToken);

        if (result is null)
        {
            return Results.NoContent();
        }

        var response = new GetNextStudyItemResponse(
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
