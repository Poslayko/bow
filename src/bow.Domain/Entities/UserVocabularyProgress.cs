namespace bow.Domain.Entities;

using bow.Domain.Enums;

public sealed class UserVocabularyProgress
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int VocabularyItemId { get; private set; }
    public LearningStage Stage { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public DateTime? LastReviewedAt { get; private set; }
    public User User { get; private set; } = null!;
    public VocabularyItem VocabularyItem { get; private set; } = null!;

    public UserVocabularyProgress(int userId, int vocabularyItemId, DateTime nextReviewAt)
    {
        if (userId <= 0 || vocabularyItemId <= 0)
        {
            throw new ArgumentException($"Wrong parameters userId: {userId}, vocabularyItemId: {vocabularyItemId}");
        }

        UserId = userId;
        VocabularyItemId = vocabularyItemId;
        Stage = LearningStage.NotStarted;
        NextReviewAt = nextReviewAt;
        LastReviewedAt = null;
    }

    public void ApplyAnswer(bool isCorrect, DateTime reviewedAt)
    {
        if (isCorrect is false)
        {
            Stage = LearningStage.NotStarted;
            LastReviewedAt = reviewedAt;
            NextReviewAt = reviewedAt;
        }

        if (isCorrect is true)
        {
            Stage = Stage switch
            {
                LearningStage.NotStarted => LearningStage.FirstCorrect,
                LearningStage.FirstCorrect => LearningStage.After3Hours,
                LearningStage.After3Hours => LearningStage.After1Day,
                LearningStage.After1Day => LearningStage.After3Days,
                LearningStage.After3Days => LearningStage.AfterWeek,
                LearningStage.AfterWeek => LearningStage.After21Days,
                LearningStage.After21Days => LearningStage.Maintenance,
                LearningStage.Maintenance => LearningStage.Maintenance,
                _ => throw new NotImplementedException($"Unsupported learning stage: {Stage}")
            };

            LastReviewedAt = reviewedAt;
            
            NextReviewAt = Stage switch
            {
                LearningStage.NotStarted => reviewedAt,
                LearningStage.FirstCorrect => reviewedAt.AddHours(1),
                LearningStage.After3Hours => reviewedAt.AddHours(3),
                LearningStage.After1Day => reviewedAt.AddDays(1),
                LearningStage.After3Days => reviewedAt.AddDays(3),
                LearningStage.AfterWeek => reviewedAt.AddDays(7),
                LearningStage.After21Days => reviewedAt.AddDays(21),
                LearningStage.Maintenance => reviewedAt.AddMonths(1),
                _ => throw new NotImplementedException($"Unsupported learning stage: {Stage}")
            };
        }
    }

    public void SetStage(LearningStage stage)
    {
        Stage = stage;
    }

    public static IReadOnlyList<CefrLevel> GetAllowedLevels(CefrLevel level)
    {
        IReadOnlyList<CefrLevel> allowedLevels = level switch
        {
            CefrLevel.A1 => [CefrLevel.A1],
            CefrLevel.A2 => [CefrLevel.A1, CefrLevel.A2],
            CefrLevel.B1 => [CefrLevel.A1, CefrLevel.A2, CefrLevel.B1],
            CefrLevel.B2 => [CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2],
            CefrLevel.C1 => [CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2,
                CefrLevel.C1],
            CefrLevel.C2 => [CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2,
                CefrLevel.C1, CefrLevel.C2],
            _ => throw new ArgumentException($"Wrong CefrLevel: {level}")
        };

        return allowedLevels;
    }
}