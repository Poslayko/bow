using bow.Domain.Enums;

namespace bow.Application.Study.StartNext;

public sealed record StartNextStudyItemResult(
    int UserVocabularyProgressId,
    int VocabularyItemId,
    string Text,
    LanguageCode Language,
    VocabularyItemType Type,
    LearningStage Stage,
    DateTime NextReviewAt,
    LanguageCode TargetLanguage
);