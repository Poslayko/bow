using bow.Domain.Entities;
using bow.Domain.Enums;

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