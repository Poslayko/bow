using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;

namespace bow.Application.VocabularyTranslations.GetBySource;

public sealed class GetVocabularyTranslationHandler
{
    private readonly IVocabularyItemRepository _item;
    private readonly IVocabularyTranslationRepository _translation;

    public GetVocabularyTranslationHandler(
        IVocabularyItemRepository vocabularyItemRepository,
        IVocabularyTranslationRepository vocabularyTranslationRepository
    )
    {
        _item = vocabularyItemRepository;
        _translation = vocabularyTranslationRepository;
    }

    public async Task<GetVocabularyTranslationsResult> HandleAsync(
        GetVocabularyTranslationQuery query,
        CancellationToken cancelationToken
    )
    {
        var item = await _item.GetByTextAndLanguageAsync(query.SourceText,
            query.SourceLanguage, cancelationToken);

        if (item is null)
        {
            throw new NotFoundException(
                $"Vocabulary item '{query.SourceText}' with language '{query.SourceLanguage}' was not found."
            );
        }

        var translations = await _translation.GetBySourceItemIdAsync(item.Id, cancelationToken);

        return new GetVocabularyTranslationsResult(
            item.Id,
            item.Text,
            item.Language,
            translations
                .Select(translation => new VocabularyTranslationResultItem(
                    translation.Id,
                    translation.TranslationToId,
                    translation.TranslationTo.Text,
                    translation.TranslationTo.Language,
                    translation.TranslationTo.Type,
                    translation.Level 
                ))
                .ToList()
        );
    }
}