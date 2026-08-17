using bow.Domain.Enums;

namespace bow.Api.Endpoints.VocabularyTranslations;

public sealed record GetVocabularyTranslationResponse(
    int SourceItemId,
    string SourceText,
    LanguageCode SourceLanguage,
    IReadOnlyList<VocabularyTranslationResponseItem> Translations
);

public sealed record VocabularyTranslationResponseItem(
    int TranslationId,
    int TargetItemId,
    string TargetText,
    LanguageCode TargetLanguage,
    VocabularyItemType TargetType,
    CefrLevel Level
);