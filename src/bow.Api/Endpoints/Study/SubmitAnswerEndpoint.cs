namespace bow.Application.Study.SubmitAnswer;

public static class SubmitAnswerEndpoint
{
    public static IEndpointRouteBuilder MapSubmitAnswerEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost(
            "/api/v1/study/answer",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        SubmitAnswerRequest request,
        SubmitStudyAnswerHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new SubmitStudyAnswerCommand(request.TelegramId, 
            request.UserVocabularyProgressId, request.Answer);

        var result = await handler.HandleAsync(command, cancellationToken);

        var response = new SubmitAnswerResponse(
            result.UserVocabularyProgressId,
            result.IsCorrect,
            result.PreviousStage,
            result.CurrentStage,
            result.LastReviewedAt,
            result.NextReviewedAt,
            result.AcceptedAnswers
        );

        return Results.Ok(response);
    }
}