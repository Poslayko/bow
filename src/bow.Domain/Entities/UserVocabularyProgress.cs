namespace bow.Domain.Entities;

using bow.Domain.Enums;

public class UserVocabularyProgress
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
}