using bow.Domain.Enums;

namespace bow.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string? DisplayName { get; private set; }
    public LanguageCode? NativeLanguage { get; private set; }
    public LanguageCode? LearningLanguage { get; private set; }
    public CefrLevel? LearningLevel { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private readonly List<UserVocabularyProgress> _userVocabularyItemProgresses = [];
    public IReadOnlyCollection<UserVocabularyProgress> UserVocabularyItemProgresses => _userVocabularyItemProgresses;
    public TelegramAccount? TelegramAccount { get; private set; }

    public User(string? displayName)
    {   
        DisplayName = displayName;
        RegisteredAt = DateTime.UtcNow;
    }
    public static User RegisterUserAsATelegramMember(string? displayName, long telegramId)
    {   
        var user = new User(displayName)
        {
            TelegramAccount = new TelegramAccount(telegramId)
        };

        return user;
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