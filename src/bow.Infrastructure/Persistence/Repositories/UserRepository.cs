using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<bool> ExistsByTelegramIdAsync(
        long telegramId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(x => x.TelegramId == telegramId, cancellationToken);
    }

    public async Task<User?> GetByTelegramIdAsync(
        long telegramId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken);
    }
}