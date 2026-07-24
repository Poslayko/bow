using bow.Domain.Enums;

namespace bow.Domain.Entities;

public class VocabularyItem
{
    public int Id { get; private set; }
    public string Text { get; private set; }
    public string NormalizedText { get; private set; }
    public LanguageCode Language { get; private set; }
    public VocabularyItemType Type { get; private set; }
    public DateTime AddedAt { get; private set; }

    private readonly List<VocabularyTranslation> _sourceTranslations = [];
    private readonly List<VocabularyTranslation> _targetTranslations = [];
    private readonly List<UserVocabularyProgress> _userVocabularyItemProgresses = [];

    public IReadOnlyCollection<VocabularyTranslation> SourceTranslations => _sourceTranslations;
    public IReadOnlyCollection<VocabularyTranslation> TargetTranslations => _targetTranslations;
    public IReadOnlyCollection<UserVocabularyProgress> UserVocabularyItemProgresses => _userVocabularyItemProgresses;

    public VocabularyItem(string text, LanguageCode language, 
        VocabularyItemType type = VocabularyItemType.Word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        Text = text.Trim();
        NormalizedText = Text.ToLowerInvariant();
        Language = language;
        AddedAt = DateTime.UtcNow;
        Type = type;
    }
}