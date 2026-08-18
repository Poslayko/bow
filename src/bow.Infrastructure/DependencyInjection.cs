using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Infrastructure.Persistence;
using bow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = 
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVocabularyItemRepository, VocabularyItemRepository>();
        services.AddScoped<IVocabularyTranslationRepository, VocabularyTranslationRepository>();
        services.AddScoped<IUserVocabularyProgressRepository, UserVocabularyProgressRepository>();
        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}