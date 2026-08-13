namespace bow.Api.Endpoints.VocabularyTranslations;

public sealed record AddVocabularyTranslationResponse(
    int VocabularyTranslationId,
    int SourceItemId,
    int TargetItemId,
    bool IsCreated,
    bool WasSourceCreated,
    bool WasTargetCreated
);