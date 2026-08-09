using bow.Domain.Entities;

namespace bow.Application.Common.Interfaces;

public interface IVocabularyTranslationRepository
{
    Task<VocabularyTranslation?> GetBySourceAndTargetAsync(
        int sourceItemId,
        int targetItemId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<VocabularyTranslation>> GetBySourceItemIdAsync(
        int sourceItemId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        VocabularyTranslation vocabularyTranslation,
        CancellationToken cancellationToken = default
    );
}