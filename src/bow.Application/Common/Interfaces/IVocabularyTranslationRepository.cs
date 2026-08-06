using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Common.Interfaces;

public interface IVocabularyTranslationRepository
{
    Task<IReadOnlyList<VocabularyTranslation>> GetBySourceItemIdAsync(
        int sourceItemId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(int sourceItemId, int targetItemId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        VocabularyTranslation vocabularyTranslation,
        CancellationToken cancellationToken = default
    );
}