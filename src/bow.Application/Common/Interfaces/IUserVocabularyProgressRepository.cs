using bow.Domain.Entities;
using bow.Domain.Enums;

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

    Task<UserVocabularyProgress?> GetNextDueAsync(
        int userId,
        LanguageCode learningLanguage,
        LanguageCode nativeLanguage,
        DateTime now,
        CancellationToken cancellationToken
    );
}