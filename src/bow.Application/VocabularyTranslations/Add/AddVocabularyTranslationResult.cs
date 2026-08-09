namespace bow.Application.VocabularyTranslations.Add;

public sealed record AddVocabularyTranslationResult(
    int VocabularyTranslationId,
    int SourceItemId,
    int TargetItemId,
    bool IsCreated,
    bool WasSourceCreated,
    bool WasTargetCreated
);