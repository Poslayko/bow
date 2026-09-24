using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;

public interface ITelegramAccountRepository
{
    Task<int?> GetUserIdAsync(
        long telegramId,
        CancellationToken token
    );

    Task<User?> GetUserByTelegramIdAsync(
        long telegramId,
        CancellationToken token
    );
}