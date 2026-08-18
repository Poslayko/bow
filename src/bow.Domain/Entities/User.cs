namespace bow.Domain.Entities;

using bow.Domain.Enums;

public class User
{
    public int Id { get; private set; }
    public long TelegramId { get; private set; }
    public string? DisplayName { get; private set; }
    public LanguageCode? NativeLanguage { get; private set; }
    public LanguageCode? LearningLanguage { get; private set; }
    public CefrLevel? LearningLevel { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private readonly List<UserVocabularyProgress> _userVocabularyItemProgresses = [];
    public IReadOnlyCollection<UserVocabularyProgress> UserVocabularyItemProgresses => _userVocabularyItemProgresses;

    public User(long telegramId, string? displayName = null)
    {   
        if (telegramId <= 0) throw new ArgumentException("User can't be without telegramId",
            nameof(telegramId));

        TelegramId = telegramId;
        DisplayName = displayName;
        RegisteredAt = DateTime.UtcNow;
    }

    public void ConfigureLearning(LanguageCode nativeLanguage, 
        LanguageCode learningLanguage, CefrLevel level)
    {
        if (nativeLanguage == learningLanguage)
        {
            throw new ArgumentException("Native language and learning language couldn't be the same");
        }

        NativeLanguage = nativeLanguage;
        LearningLanguage = learningLanguage;
        LearningLevel = level;
    }
}