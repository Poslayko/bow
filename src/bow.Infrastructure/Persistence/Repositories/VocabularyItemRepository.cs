using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

public class VocabularyItemRepository : IVocabularyItemRepository
{
    private readonly AppDbContext _appDbContext;

    public VocabularyItemRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task AddAsync(VocabularyItem vocabularyItem, 
        CancellationToken cancellationToken = default)
    {
        await _appDbContext.VocabularyItems.AddAsync(vocabularyItem, cancellationToken);
    }

    public Task<bool> ExistsByTextAndLanguageAsync(string text, LanguageCode language, 
        CancellationToken cancellationToken = default)
    {
        var normalizedText = VocabularyItem.NormalizeText(text);
        return _appDbContext.VocabularyItems.AnyAsync(x => x.Language == language &&
            x.NormalizedText == normalizedText, cancellationToken);
    }

    public Task<VocabularyItem?> GetByTextAndLanguageAsync(string text, 
        LanguageCode language, CancellationToken cancellationToken = default)
    {
        var normalizedText = VocabularyItem.NormalizeText(text);
        
        return _appDbContext.VocabularyItems.SingleOrDefaultAsync(x => x.Language == language &&
            x.NormalizedText == normalizedText, cancellationToken);

    }
}