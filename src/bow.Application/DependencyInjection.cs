using bow.Application.Telegram;
using bow.Application.Telegram.Onboarding;
using bow.Application.Telegram.Registration;
using bow.Application.Telegram.Study.StartNext;
using bow.Application.Telegram.Study.SubmitAnswer;
using bow.Application.Users.ConfigureLearning;
using bow.Application.UserVocabularyProgresses.Add;
using bow.Application.VocabularyItems.Add;
using bow.Application.VocabularyTranslations.Add;
using bow.Application.VocabularyTranslations.GetBySource;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<HandleTelegramInputHandler>();
        services.AddScoped<SendPossibleNativeLanguageHandler>();
        services.AddScoped<AddVocabularyItemHandler>();
        services.AddScoped<AddVocabularyTranslationHandler>();
        services.AddScoped<GetVocabularyTranslationHandler>();
        services.AddScoped<AddUserVocabularyProgressHandler>();
        services.AddScoped<StartNextStudyItemHandler>();
        services.AddScoped<SubmitStudyAnswerHandler>();
        services.AddScoped<RegisterTelegramUserHandler>();
        services.AddScoped<ChooseNativeLanguageHandler>();
        services.AddScoped<SelectNativeLanguageHandler>();
        services.AddScoped<ChooseLearningLanguageHandler>();
        services.AddScoped<SelectLearningLanguageHandler>();
        services.AddScoped<ChooseLearningLevelHandler>();
        services.AddScoped<SelectLearningLevelHandler>();
        services.AddScoped<ShowMenuHandler>();
        services.AddScoped<ShowLearningMenuHandler>();
        
        return services;
    }
}
