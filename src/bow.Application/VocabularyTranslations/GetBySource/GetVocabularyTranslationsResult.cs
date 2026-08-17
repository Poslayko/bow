using bow.Domain.Enums;

namespace bow.Application.VocabularyTranslations.GetBySource;

public sealed record GetVocabularyTranslationsResult(
    int SourceItemId,
    string SourceText,
    LanguageCode SourceLanguage,
    IReadOnlyList<VocabularyTranslationResultItem> Translations
);

public sealed record VocabularyTranslationResultItem(
    int TranslationId,
    int TargetItemId,
    string TargetText,
    LanguageCode TargetLanguage,
    VocabularyItemType TargetType,
    CefrLevel Level
);