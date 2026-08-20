using bow.Domain.Entities;
using bow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using bow.Domain.Enums;

namespace bow.Infrastructure.Persistence.Repositories;

internal sealed class UserVocabularyProgressRepository : IUserVocabularyProgressRepository
{
    private readonly AppDbContext _db;

    public UserVocabularyProgressRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(UserVocabularyProgress userVocabularyProgress, 
        CancellationToken cancellationToken = default)
    {
        await _db.UserVocabularyProgresses.AddAsync(userVocabularyProgress, 
            cancellationToken);
    }

    public async Task<UserVocabularyProgress?> GetByUserAndVocabularyItemAsync(int userId, 
        int vocabularyItemId, CancellationToken cancellationToken)
    {
        return await _db.UserVocabularyProgresses
            .SingleOrDefaultAsync(x => x.UserId == userId &&
                x.VocabularyItemId == vocabularyItemId, cancellationToken);
    }

    public async Task<UserVocabularyProgress?> GetNextDueAsync(
        int userId, 
        LanguageCode learningLanguage, 
        LanguageCode nativeLanguage, 
        DateTime now, 
        CancellationToken cancellationToken
    )
    {
        var item = await _db.UserVocabularyProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == userId
                && progress.VocabularyItem.Language == learningLanguage
                && progress.VocabularyItem.SourceTranslations.Any(item => 
                    item.TranslationTo.Language == nativeLanguage)
                && progress.NextReviewAt <= now)
            .Include(progress => progress.VocabularyItem)
            .OrderBy(progress => progress.Stage == LearningStage.NotStarted)
            .ThenBy(progress => progress.NextReviewAt)
            .ThenBy(progress => progress.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return item;
    }
}