using bow.Domain.Enums;

namespace bow.Domain.Entities;

public class VocabularyTranslation
{
    public int Id { get; private set; }
    public int TranslationFromId { get; private set; }
    public int TranslationToId { get; private set; }
    public CefrLevel Level { get; private set; }
    public DateTime AddedAt { get; private set; }
    public VocabularyItem TranslationFrom { get; private set; } = null!;
    public VocabularyItem TranslationTo { get; private set; } = null!;

    public VocabularyTranslation(int translationFromId, int translationToId, CefrLevel level = CefrLevel.A1)
    {
        if (translationFromId <= 0)
            throw new ArgumentOutOfRangeException(nameof(translationFromId));

        if (translationToId <= 0)
            throw new ArgumentOutOfRangeException(nameof(translationToId));

        if (translationFromId == translationToId)
            throw new ArgumentException("TranslationFrom to TranslationTo cannot be the same.");

        TranslationFromId = translationFromId;
        TranslationToId = translationToId;
        AddedAt = DateTime.UtcNow;
        Level = level;
    }
}