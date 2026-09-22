using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Api.IntegrationTests.Infrastructure;

public static class TestDataFactory
{
    public static async Task<User> CreateUserAsync(IUserRepository users, IUnitOfWork unit,
        long telegramId)
    {
        var user = new User(telegramId);

        await users.AddAsync(user);

        await unit.SaveChangesAsync();

        return user;
    }

    public static async Task ConfigureLearningAsync(
        User user, 
        IUnitOfWork unit,
        LanguageCode nativeLanguage = LanguageCode.Ru,
        LanguageCode learningLanguage = LanguageCode.En,
        CefrLevel learningLevel = CefrLevel.B1)
    {
        user.ConfigureLearning(nativeLanguage, learningLanguage, learningLevel); 

        await unit.SaveChangesAsync(); 
    }

    public static async Task<VocabularyItem> CreateItemAsync(
        IVocabularyItemRepository items,
        IUnitOfWork unit,
        String text,
        LanguageCode language,
        VocabularyItemType type = VocabularyItemType.Word
    )
    {
        var item = new VocabularyItem(text, language, type);

        await items.AddAsync(item, CancellationToken.None);

        await unit.SaveChangesAsync();

        return item;
    }

    public static async Task<VocabularyTranslation> CreateTranslationAsync(
        IVocabularyTranslationRepository translations,
        IUnitOfWork unit,
        VocabularyItem sourceItem,
        VocabularyItem targetItem,
        CefrLevel level = CefrLevel.A1
    )
    {
        var translation = new VocabularyTranslation(sourceItem, targetItem, level);

        await translations.AddAsync(translation);
        
        await unit.SaveChangesAsync();

        return translation;
    }
    
    public static async Task<UserVocabularyProgress> CreateProgressAsync(
        IUserVocabularyProgressRepository progresses,
        IUnitOfWork unit,
        int userId,
        int vocabularyItemId,
        DateTime now
    )
    {
        var progress = new UserVocabularyProgress(userId, vocabularyItemId, now);

        await progresses.AddAsync(progress);

        await unit.SaveChangesAsync();

        return progress;
    }
}