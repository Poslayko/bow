using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

public sealed class TelegramAccountRepository : ITelegramAccountRepository
{
    private readonly AppDbContext _db;

    public TelegramAccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetUserByTelegramIdAsync(long telegramId, CancellationToken token)
    {
        return _db.Users
            .Include(x => x.TelegramAccount)
            .Where(x => x.TelegramAccount != null &&
                x.TelegramAccount.TelegramId == telegramId)
            .SingleOrDefaultAsync(token);
    }

    public async Task<int?> GetUserIdAsync(long telegramId, CancellationToken token)
    {
        return await _db.TelegramAccounts
            .Where(x => x.TelegramId == telegramId)
            .Select(x => x.UserId)
            .SingleOrDefaultAsync(token);
    }
}