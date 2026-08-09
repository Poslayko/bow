using bow.Application.Common.Interfaces;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.VocabularyTranslations.Add;

public sealed class AddVocabularyTranslationHandler
{
    private readonly IVocabularyItemRepository _itemRepository;
    private readonly IVocabularyTranslationRepository _translationRepository;
    private readonly IUnitOfWork _unit;

    public AddVocabularyTranslationHandler(
        IVocabularyItemRepository itemRepository,
        IVocabularyTranslationRepository translationRepository,
        IUnitOfWork unit)
    {
        _itemRepository = itemRepository;
        _translationRepository = translationRepository;
        _unit = unit;
    }

    public async Task<AddVocabularyTranslationResult> HandleAsync(
        AddVocabularyTranslationCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.SourceLanguage ==
            command.TargetLanguage)
        {
            throw new ArgumentException("Language cannot be the same");
        }

        var (sourceItem, wasSourceCreated) = await GetOrCreateItemAsync(
            command.SourceText,
            command.SourceLanguage,
            command.SourceType,
            cancellationToken
        );

        var (targetItem, wasTargetCreated) = await GetOrCreateItemAsync(
            command.TargetText,
            command.TargetLanguage,
            command.TargetType,
            cancellationToken
        );

        bool isCreated = false;

        var translation = wasSourceCreated || wasTargetCreated
            ? null
            : await _translationRepository.GetBySourceAndTargetAsync(
                sourceItem.Id, 
                targetItem.Id, 
                cancellationToken);

        if (translation is null)
        {
            translation = new VocabularyTranslation(sourceItem, targetItem,
                command.Level);
            
            await _translationRepository.AddAsync(translation, cancellationToken);

            await _unit.SaveChangesAsync(cancellationToken);

            isCreated = true;
        }

        return new AddVocabularyTranslationResult(
            translation.Id,
            sourceItem.Id,
            targetItem.Id,
            isCreated,
            wasSourceCreated,
            wasTargetCreated
        );
    }

    private async Task<(VocabularyItem Item, bool WasCreated)> GetOrCreateItemAsync(
        string text,
        LanguageCode language,
        VocabularyItemType type,
        CancellationToken cancellationToken
    )
    {
        var item = await _itemRepository.GetByTextAndLanguageAsync(
            text,
            language,
            cancellationToken
        );

        if (item is not null)
        {
            return (item, false);
        }

        item = new VocabularyItem(text, language, type);

        await _itemRepository.AddAsync(item, cancellationToken);

        return (item, true);
    }
}