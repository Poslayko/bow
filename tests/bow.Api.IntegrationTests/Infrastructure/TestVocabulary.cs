using bow.Domain.Entities;
using bow.Domain.Enums;
using bow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bow.Api.IntegrationTests.Infrastructure;

public static class TestVocabulary
{
    public static async Task SeedAsync(
        AppDbContext db, 
        CancellationToken cancellationToken = default)
    {
        await CreatePair(db, "lezen", "читати", cancellationToken, CefrLevel.A1);
        await CreatePair(db, "schrijven", "писати", cancellationToken, CefrLevel.A1);
        await CreatePair(db, "rennen", "бігти", cancellationToken, CefrLevel.A1);
        await CreatePair(db, "afspraak", "домовленість", cancellationToken, CefrLevel.A2);
        await CreatePair(db, "afspraak", "зустріч", cancellationToken, CefrLevel.A2);
        await CreatePair(db, "ervaring", "досвід", cancellationToken, CefrLevel.B1);
        await CreatePair(db, "voorwaarde", "умова", cancellationToken, CefrLevel.B2);
        await CreatePair(db, "vanzelfsprekend", "само собою зрозумілий", cancellationToken, 
            CefrLevel.C1, targetType: VocabularyItemType.Phrase);

        await db.SaveChangesAsync();
    }

    private static async Task CreatePair(
        AppDbContext db, 
        string sourceText, 
        string translationText,
        CancellationToken cancellationToken,
        CefrLevel level = CefrLevel.A1,
        VocabularyItemType sourceType = VocabularyItemType.Word,
        VocabularyItemType targetType = VocabularyItemType.Word
    )
    {
        var sourceItem = db.VocabularyItems.Local.SingleOrDefault(x => x.Text == sourceText);

        if (sourceItem is null)
        {
            sourceItem = new VocabularyItem(sourceText, LanguageCode.Nl, sourceType);
            await db.VocabularyItems.AddAsync(sourceItem, cancellationToken);
        }

        var targetItem = db.VocabularyItems.Local.SingleOrDefault(x => x.Text == translationText);

        if (targetItem is null)
        {
            targetItem = new VocabularyItem(translationText, LanguageCode.Ua, targetType);
            await db.VocabularyItems.AddAsync(targetItem, cancellationToken);
        }
        
        var translation = new VocabularyTranslation(sourceItem, targetItem, level);

        await db.VocabularyTranslations.AddAsync(translation);
    }
}