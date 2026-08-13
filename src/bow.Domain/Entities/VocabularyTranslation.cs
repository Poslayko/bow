using bow.Domain.Enums;

namespace bow.Domain.Entities;

public sealed class VocabularyTranslation
{
    public int Id { get; private set; }
    public int TranslationFromId { get; private set; }
    public int TranslationToId { get; private set; }
    public CefrLevel Level { get; private set; }
    public DateTime AddedAt { get; private set; }
    public VocabularyItem TranslationFrom { get; private set; } = null!;
    public VocabularyItem TranslationTo { get; private set; } = null!;
    public VocabularyTranslation()
    {

    }

    public VocabularyTranslation(VocabularyItem sourceItem, VocabularyItem targetItem, 
        CefrLevel level = CefrLevel.A1)
    {
        ArgumentNullException.ThrowIfNull(sourceItem);
        ArgumentNullException.ThrowIfNull(targetItem);

        if (ReferenceEquals(sourceItem, targetItem))
            throw new ArgumentException("Translation items cannot be the same.");

        TranslationFrom = sourceItem;
        TranslationTo = targetItem;
        AddedAt = DateTime.UtcNow;
        Level = level;
    }
}