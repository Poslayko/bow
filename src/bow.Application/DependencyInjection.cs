using bow.Application.Study.GetNext;
using bow.Application.Users.ConfigureLearning;
using bow.Application.Users.Register;
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
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<AddVocabularyItemHandler>();
        services.AddScoped<AddVocabularyTranslationHandler>();
        services.AddScoped<GetVocabularyTranslationHandler>();
        services.AddScoped<AddUserVocabularyProgressHandler>();
        services.AddScoped<ConfigureLearningUserHandler>();
        services.AddScoped<GetNextStudyItemHandler>();
        
        return services;
    }
}
