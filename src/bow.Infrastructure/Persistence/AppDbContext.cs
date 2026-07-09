namespace bow.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using bow.Domain.Entities;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserWordProgress> UserWordProgresses => Set<UserWordProgress>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<WordTranslation> WordTranslations => Set<WordTranslation>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}