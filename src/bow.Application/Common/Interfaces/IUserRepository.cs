using bow.Domain.Entities;
using bow.Domain.Enums;

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

    Task SetNativeLanguage(
        User user,
        LanguageCode language,
        CancellationToken cancellationToken = default
    );
}