using bow.Domain.Entities;
using bow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bow.Infrastructure.Persistence.Repositories;

internal sealed class UserVocabularyProgressRepository : IUserVocabularyProgressRepository
{
    private readonly AppDbContext _db;

    public UserVocabularyProgressRepository(AppDbContext db)
    {
        _db = db;
    }

    public async  Task AddAsync(UserVocabularyProgress userVocabularyProgress, 
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
}