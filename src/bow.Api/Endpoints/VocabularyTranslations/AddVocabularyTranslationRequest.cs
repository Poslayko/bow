using bow.Domain.Enums;

namespace bow.Api.Endpoints.VocabularyTranslations;

public sealed record AddVocabularyTranslationRequest(
    string SourceText,
    LanguageCode SourceLanguage,
    VocabularyItemType SourceType,
    string TargetText,
    LanguageCode TargetLanguage,
    VocabularyItemType TargetType,
    CefrLevel Level
);