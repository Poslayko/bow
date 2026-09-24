using bow.Application.Common.Interfaces;
using bow.Application.Study.SubmitAnswer;

namespace bow.Api.Endpoints.Study;

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
        ITelegramAccountRepository telegramAccountRepository,
        CancellationToken token
    )
    {
        var possibleUserId = await telegramAccountRepository.GetUserIdAsync(request.TelegramId, token);

        if (possibleUserId is not {} userId)
        {
            return Results.NotFound($"User with TelegramId: {request.TelegramId} wasn't found");
        }

        var command = new SubmitStudyAnswerCommand(userId, 
            request.UserVocabularyProgressId, request.Answer);

        var result = await handler.HandleAsync(command, token);

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