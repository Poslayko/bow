using bow.Application.UserVocabularyProgresses.Add;

namespace bow.Api.Endpoints.UserVocabularyProgress;

public static class AddUserVocabularyProgressEndpoint
{
    public static IEndpointRouteBuilder MapAddUserVocabularyProgressEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost(
            "/api/v1/user-vocabulary-progress",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        AddUserVocabularyProgressRequest request,
        AddUserVocabularyProgressHandler handler,
        CancellationToken token
    )
    {
        var command = new AddUserVocabularyProgressCommand(
            request.TelegramId,
            request.VocabularyItemId
        );

        var result = await handler.HandleAsync(command, token);

        var response = new AddUserVocabularyProgressResponse(
            result.IsCreated,
            result.UserVocabularyProgressId,
            result.VocabularyItemId,
            result.UserId,
            result.Stage,
            result.NextReviewAt,
            result.LastReviewedAt
        );

        if (response.IsCreated)
        {
            return Results.Created(
                $"/api/v1/user-vocabulary-progress/{response.UserVocabularyProgressId}",
                response);
        }

        return Results.Ok(response);
    }
}