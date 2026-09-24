using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByIdAsync(
        int userId,
        CancellationToken cancellationToken = default
    );
}