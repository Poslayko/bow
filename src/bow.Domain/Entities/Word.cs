namespace bow.Domain.Entities;

using bow.Domain.Enums;

public class Word
{
    public int Id { get; private set; }
    public string Text { get; private set; }
    public LanguageCode Language { get; private set; }
    public CefrLevel Level { get; private set; }
    public DateTime AddedAt { get; private set; }

    public Word(string text, LanguageCode language, CefrLevel level = CefrLevel.A1)
    {
        if (string.IsNullOrWhiteSpace(text)) 
            throw new ArgumentException("Word cannot be empty.", nameof(text));
        Text = text;
        Language = language;
        Level = level;
        AddedAt = DateTime.UtcNow;
    }
}