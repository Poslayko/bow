using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

internal sealed class VocabularyTranslationRepository : IVocabularyTranslationRepository
{
    private readonly AppDbContext _db;

    public VocabularyTranslationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(VocabularyTranslation vocabularyTranslation,
        CancellationToken cancellationToken = default)
    {
        await _db.VocabularyTranslations.AddAsync(vocabularyTranslation, cancellationToken);
    }

    public Task<bool> ExistsAsync(int sourceItemId, int targetItemId,
        CancellationToken cancellationToken = default)
    {
        return _db.VocabularyTranslations.AnyAsync(x => 
            x.TranslationFromId == sourceItemId && x.TranslationToId == targetItemId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<VocabularyTranslation>> GetBySourceItemIdAsync(
        int sourceItemId, 
        CancellationToken cancellationToken = default)
    {
        return await _db.VocabularyTranslations
            .Where(x => x.TranslationFromId == sourceItemId)
            .ToListAsync(cancellationToken);
            
    }
}