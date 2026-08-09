using bow.Application.Users.Register;
using bow.Application.VocabularyItems.Add;
using bow.Application.VocabularyTranslations.Add;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<AddVocabularyItemHandler>();
        services.AddScoped<AddVocabularyTranslationHandler>();

        return services;
    }
}