using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

public class VocabularyItemRepository : IVocabularyItemRepository
{
    private readonly AppDbContext _db;

    public VocabularyItemRepository(AppDbContext appDbContext)
    {
        _db = appDbContext;
    }

    public async Task AddAsync(VocabularyItem vocabularyItem, 
        CancellationToken cancellationToken = default)
    {
        await _db.VocabularyItems.AddAsync(vocabularyItem, cancellationToken);
    }

    public Task<bool> ExistsByTextAndLanguageAsync(string text, LanguageCode language, 
        CancellationToken cancellationToken = default)
    {
        var normalizedText = VocabularyItem.NormalizeText(text);
        return _db.VocabularyItems.AnyAsync(x => x.Language == language &&
            x.NormalizedText == normalizedText, cancellationToken);
    }

    public Task<VocabularyItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.VocabularyItems
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<VocabularyItem?> GetByTextAndLanguageAsync(string text, 
        LanguageCode language, CancellationToken cancellationToken = default)
    {
        var normalizedText = VocabularyItem.NormalizeText(text);
        
        return _db.VocabularyItems.SingleOrDefaultAsync(x => x.Language == language &&
            x.NormalizedText == normalizedText, cancellationToken);
    }
    public async Task<int?> TryToGetPossibleNextItemIdAsync(
        int userId,
        IReadOnlyList<CefrLevel> allowedLevels,
        LanguageCode learningLanguage,
        LanguageCode nativeLanguage,
        CancellationToken cancellationToken)
    {
        var itemId = await _db.VocabularyItems
            .Where(item => item.Language == learningLanguage)
            .Where(item => !_db.UserVocabularyProgresses
                .Any(progress => progress.VocabularyItemId == item.Id 
                    && progress.UserId == userId))
            .Where(item => _db.VocabularyTranslations
                .Any(translation => translation.TranslationFromId == item.Id 
                    && translation.TranslationTo.Language == nativeLanguage
                    && allowedLevels.Contains(translation.Level)))
            .OrderBy(item => item.Id)
            .Select(item => (int?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return itemId;
    }
}