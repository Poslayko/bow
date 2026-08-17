
using bow.Application.VocabularyTranslations.GetBySource;

namespace bow.Api.Endpoints.VocabularyTranslations;

public static class GetVocabularyTranslationEndpoint
{
    public static IEndpointRouteBuilder MapGetVocabularyTranslationEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapGet(
            "/api/v1/vocabulary-translations",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        [AsParameters] GetVocabularyTranslationRequest request,
        GetVocabularyTranslationHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetVocabularyTranslationQuery(
            request.SourceText, 
            request.SourceLanguage
        );

        var result = await handler.HandleAsync(query, cancellationToken);

        var response = new GetVocabularyTranslationResponse(
            result.SourceItemId,
            result.SourceText,
            result.SourceLanguage,
            result.Translations
                .Select(x => new VocabularyTranslationResponseItem(
                    x.TranslationId,
                    x.TargetItemId,
                    x.TargetText,
                    x.TargetLanguage,
                    x.TargetType,
                    x.Level
                ))
                .ToList()
        );

        return Results.Ok(response);
    }
}
