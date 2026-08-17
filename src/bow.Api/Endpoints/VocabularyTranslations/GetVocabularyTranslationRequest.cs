using bow.Domain.Enums;

namespace bow.Api.Endpoints.VocabularyTranslations;

public sealed record GetVocabularyTranslationRequest(
    string SourceText,
    LanguageCode SourceLanguage
);