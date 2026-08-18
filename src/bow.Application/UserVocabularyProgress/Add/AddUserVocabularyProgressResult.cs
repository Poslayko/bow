using bow.Domain.Enums;

namespace bow.Application.UserVocabularyProgresses.Add;

public sealed record AddUserVocabularyProgressResult(
    bool IsCreated,
    int UserVocabularyProgressId,
    LearningStage Stage,
    int UserId,
    DateTime NextReviewAt
);