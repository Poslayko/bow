using bow.Domain.Enums;

namespace bow.Api.Endpoints.Study;

public sealed record SubmitAnswerResponse(
    int UserVocabularyProgressId,
    bool IsCorrect,
    LearningStage PreviousStage,
    LearningStage CurrentStage,
    DateTime? LastReviewedAt,
    DateTime NextReviewedAt,
    IReadOnlyList<string> AcceptedAnswers
);