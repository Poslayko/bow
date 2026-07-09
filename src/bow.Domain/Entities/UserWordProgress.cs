namespace bow.Domain.Entities;

using bow.Domain.Enums;

public class UserWordProgress
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int WordId { get; private set; }
    public LearningStage Stage { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public DateTime? LastReviewedAt { get; private set; }

    public UserWordProgress(int userId, int wordId, DateTime nextReviewAt)
    {
        if (userId <= 0 || wordId <= 0)
        {
            throw new ArgumentException($"Wrong parameters userId: {userId}, wordId: {wordId}");
        }

        UserId = userId;
        WordId = wordId;
        Stage = LearningStage.NotStarted;
        NextReviewAt = nextReviewAt;
        LastReviewedAt = null;
    }
}