using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Common.Interfaces;

public interface IVocabularyItemRepository
{
    Task<VocabularyItem?> GetByTextAndLanguageAsync(
        string text,
        LanguageCode language,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByTextAndLanguageAsync(
        string text,
        LanguageCode language,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        VocabularyItem vocabularyItem,
        CancellationToken cancellationToken = default
    );
}