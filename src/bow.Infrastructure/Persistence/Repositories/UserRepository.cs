using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AnyAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task SetNativeLanguage(User user, LanguageCode language, 
        CancellationToken cancellationToken)
    {
        user.SetNativeLanguage(language);

        await _db.SaveChangesAsync(cancellationToken);
    }

}