using bow.Domain.Enums;

namespace bow.Application.VocabularyTranslations.Add;

public sealed record AddVocabularyTranslationCommand(
    string SourceText,
    LanguageCode SourceLanguage,
    VocabularyItemType SourceType,
    string TargetText,
    LanguageCode TargetLanguage,
    VocabularyItemType TargetType,
    CefrLevel Level
);