using bow.Domain.Enums;

namespace bow.Api.Endpoints.UserVocabularyProgress;

public sealed record AddUserVocabularyProgressResponse(
    bool IsCreated,
    int UserVocabularyProgressId,
    int VocabularyItemId,
    int UserId,
    LearningStage Stage,
    DateTime NextReviewAt,
    DateTime? LastReviewedAt
);