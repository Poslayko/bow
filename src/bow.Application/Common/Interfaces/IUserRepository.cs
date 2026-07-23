using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByTelegramIdAsync(
        long telegramId,
        CancellationToken cancellationToken = default
    );

    public Task AddAsync(
        User user,
        CancellationToken cancellationToken = default
    );

    public Task<bool> ExistsByTelegramIdAsync(
        long telegramId,
        CancellationToken cancellationToken
    );
}