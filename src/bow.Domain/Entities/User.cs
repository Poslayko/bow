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
    public static User RegisterUserWithTelegramData(string? userName, long telegramId)
    {   
        var user = new User(userName)
        {
            TelegramAccount = new TelegramAccount(telegramId, userName)
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

    public void SetNativeLanguage(LanguageCode nativeLanguage)
    {
        NativeLanguage = nativeLanguage;
    }

    public void SetLearningLanguage(LanguageCode learningLanguage)
    {
        LearningLanguage = learningLanguage;
    }

    public void SetLearningLevel(CefrLevel cefrLevel)
    {
        LearningLevel = cefrLevel;
    }
}