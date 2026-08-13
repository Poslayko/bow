using bow.Application.VocabularyTranslations.Add;

namespace bow.Api.Endpoints.VocabularyTranslations;

public static class AddVocabularyTranslationEndpoint
{
    public static IEndpointRouteBuilder MapAddVocabularyTranslationEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost(
            "/api/v1/vocabulary-translations",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        AddVocabularyTranslationRequest request,
        AddVocabularyTranslationHandler handler,
        CancellationToken token
    )
    {
        var addTranslationCommand = new AddVocabularyTranslationCommand(
            request.SourceText,
            request.SourceLanguage,
            request.SourceType,
            request.TargetText,
            request.TargetLanguage,
            request.TargetType,
            request.Level
        );

        var result = await handler.HandleAsync(addTranslationCommand, token);

        var response = new AddVocabularyTranslationResponse(
            result.VocabularyTranslationId,
            result.SourceItemId,
            result.TargetItemId,
            result.IsCreated,
            result.WasSourceCreated,
            result.WasTargetCreated
        );

        if (result.IsCreated)
        {
            return Results.Created(
                $"/api/v1/vocabulary-translations/{response.VocabularyTranslationId}",
                response);
        }

        return Results.Ok(response);

    }
}