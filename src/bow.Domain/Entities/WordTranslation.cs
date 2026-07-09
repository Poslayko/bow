namespace bow.Domain.Entities;

public class WordTranslation
{
    public int Id { get; private set; }
    public int SourceWordId { get; private set; }
    public int TargetWordId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public WordTranslation(int sourceWordId, int targetWordId)
    {
        if (sourceWordId <= 0 || targetWordId <= 0 || sourceWordId == targetWordId)
        {
            throw new ArgumentException($"Wrong parameters: sourceWordId: {sourceWordId},",
                $"targetWordId: {targetWordId}");
        }
        SourceWordId = sourceWordId;
        TargetWordId = targetWordId;
        AddedAt = DateTime.UtcNow;
    }
}