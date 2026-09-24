using bow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bow.Api.IntegrationTests.Infrastructure;

public static class TestDatabase
{
    public static async Task ResetTables(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
                user_vocabulary_progresses,
                vocabulary_translations,
                vocabulary_items,
                telegram_accounts,
                users
            RESTART IDENTITY CASCADE;
            """);
    }

    public static T GetService<T>(IServiceScope scope) where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}