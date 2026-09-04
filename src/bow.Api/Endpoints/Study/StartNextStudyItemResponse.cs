using bow.Domain.Enums;

namespace bow.Api.Endpoints.Study;

public sealed record StartNextStudyItemResponse(
    int UserVocabularyProgressId,
    int VocabularyItemId,
    string Text,
    LanguageCode Language,
    VocabularyItemType Type,
    LearningStage Stage,
    DateTime NextReviewAt,
    LanguageCode TargetLanguage
);