using bow.Domain.Enums;

namespace bow.Application.VocabularyTranslations.GetBySource;

public sealed record GetVocabularyTranslationQuery(
    string SourceText,
    LanguageCode SourceLanguage
);