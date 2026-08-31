using bow.Domain.Enums;

namespace bow.Application.Study.SubmitAnswer;

public sealed record SubmitAnswerResponse(
    int UserVocabularyProgressId,
    bool IsCorrect,
    LearningStage PreviousStage,
    LearningStage CurrentStage,
    DateTime? LastReviewedAt,
    DateTime NextReviewedAt,
    IReadOnlyList<string> AcceptedAnswers
);