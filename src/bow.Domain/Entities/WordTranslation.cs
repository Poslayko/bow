namespace bow.Domain.Entities;

public class WordTranslation
{
    public int Id { get; private set; }
    public int SourceWordId { get; private set; }
    public int TargetWordId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public Word SourceWord { get; private set; } = null!;
    public Word TargetWord { get; private set; } = null!;

    public WordTranslation(int sourceWordId, int targetWordId)
    {
        if (sourceWordId <= 0)
            throw new ArgumentOutOfRangeException(nameof(sourceWordId));

        if (targetWordId <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetWordId));

        if (sourceWordId == targetWordId)
            throw new ArgumentException("Source and target words cannot be the same.");

        SourceWordId = sourceWordId;
        TargetWordId = targetWordId;
        AddedAt = DateTime.UtcNow;
    }
}