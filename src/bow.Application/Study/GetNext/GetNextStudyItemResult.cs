using bow.Domain.Enums;

namespace bow.Application.Study.GetNext;

public sealed record GetNextStudyItemResult(
    int UserVocabularyProgressId,
    int VocabularyItemId,
    string Text,
    LanguageCode Language,
    VocabularyItemType Type,
    LearningStage Stage,
    DateTime NextReviewAt,
    LanguageCode TargetLanguage
);