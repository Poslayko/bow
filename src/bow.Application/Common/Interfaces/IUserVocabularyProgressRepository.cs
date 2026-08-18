using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;
public interface IUserVocabularyProgressRepository
{
    Task<UserVocabularyProgress?> GetByUserAndVocabularyItemAsync(
        int userId,
        int vocabularyItemId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        UserVocabularyProgress userVocabularyProgress,
        CancellationToken cancellationToken = default
    );
}