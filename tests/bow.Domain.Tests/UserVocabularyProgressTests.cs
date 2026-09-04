using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Domain.Tests;

public class UserVocabularyProgressTests
{
    [Fact]
    public void ApplyAnswer_WhenAnswerIsWrong_ResetsStageToNotStarted()
    {
        var progress = new UserVocabularyProgress(
            userId: 1,
            vocabularyItemId: 10,
            nextReviewAt: DateTime.UtcNow
        );

        progress.SetStage(LearningStage.After3Hours);

        var reviewedAt = new DateTime(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc);

        progress.ApplyAnswer(isCorrect: false, reviewedAt);

        Assert.Equal(LearningStage.NotStarted, progress.Stage);
        Assert.Equal(reviewedAt, progress.LastReviewedAt);
        Assert.Equal(reviewedAt, progress.NextReviewAt);
    }

    [Theory]
    [InlineData(LearningStage.NotStarted, LearningStage.FirstCorrect)]
    [InlineData(LearningStage.FirstCorrect, LearningStage.After3Hours)]
    [InlineData(LearningStage.After3Hours, LearningStage.After1Day)]
    [InlineData(LearningStage.After1Day, LearningStage.After3Days)]
    [InlineData(LearningStage.After3Days, LearningStage.AfterWeek)]
    [InlineData(LearningStage.AfterWeek, LearningStage.After21Days)]
    [InlineData(LearningStage.After21Days, LearningStage.Maintenance)]
    [InlineData(LearningStage.Maintenance, LearningStage.Maintenance)]
    public void ApplyAnswer_WhenCorrect_MovesToNextStage(
        LearningStage initialStage,
        LearningStage expectedStage
    )
    {
        var reviewedAt = new DateTime(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc);
        
        var progress = new UserVocabularyProgress(
            23,
            231,
            reviewedAt
        );

        progress.SetStage(initialStage);


        var nextReviewAt = progress.Stage switch {
            LearningStage.NotStarted => reviewedAt.AddHours(1),
            LearningStage.FirstCorrect => reviewedAt.AddHours(3),
            LearningStage.After3Hours => reviewedAt.AddDays(1),
            LearningStage.After1Day => reviewedAt.AddDays(3),
            LearningStage.After3Days => reviewedAt.AddDays(7),
            LearningStage.AfterWeek => reviewedAt.AddDays(21),
            LearningStage.After21Days => reviewedAt.AddMonths(1),
            LearningStage.Maintenance => reviewedAt.AddMonths(1),
            _ => reviewedAt 
        };

        progress.ApplyAnswer(true, reviewedAt);

        Assert.Equal(expectedStage, progress.Stage);
        Assert.Equal(reviewedAt, progress.LastReviewedAt);
        Assert.Equal(nextReviewAt, progress.NextReviewAt);
    }

    public static IEnumerable<object[]> AllowedLevelsCases()
    {
        yield return new object[]
        {
            CefrLevel.A1,
            new[] { CefrLevel.A1 }
        };

        yield return new object[]
        {
            CefrLevel.A2,
            new[] { CefrLevel.A1, CefrLevel.A2 }
        };

        yield return new object[]
        {
            CefrLevel.B1,
            new[] { CefrLevel.A1, CefrLevel.A2, CefrLevel.B1 }
        };

        yield return new object[]
        {
            CefrLevel.B2,
            new[] { CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2 }
        };

        yield return new object[]
        {
            CefrLevel.C1,
            new[] { CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2, CefrLevel.C1 }
        };

        yield return new object[]
        {
            CefrLevel.C2,
            new[] { CefrLevel.A1, CefrLevel.A2, CefrLevel.B1, CefrLevel.B2, CefrLevel.C1, CefrLevel.C2 }
        };
    }

    [Theory]
    [MemberData(nameof(AllowedLevelsCases))]
    public void GetAllowedLevels_WhenExist_GetListOfAllowedLevels(
        CefrLevel level,
        IReadOnlyCollection<CefrLevel> expectedLevels
    )
    {
        var allowedLevels = UserVocabularyProgress.GetAllowedLevels(level);

        Assert.Equal(expectedLevels, allowedLevels);
    }
}
