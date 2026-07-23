using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(
        long telegramId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByTelegramIdAsync(
        long telegramId,
        CancellationToken cancellationToken = default
    );
}