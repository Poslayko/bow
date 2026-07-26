using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.VocabularyItems.Add;

public sealed class AddVocabularyItemHandler
{
    private readonly IVocabularyItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddVocabularyItemHandler(IVocabularyItemRepository vocabularyItemRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = vocabularyItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddVocabularyItemResult> HandleAsync(
        AddVocabularyItemCommand addVocabularyItemCommand, 
        CancellationToken cancellationToken
    )
    {
        var existingItem = await _repository.GetByTextAndLanguageAsync(
            addVocabularyItemCommand.Text,
            addVocabularyItemCommand.Language,
            cancellationToken
        );

        if (existingItem is not null)
        {
            return new AddVocabularyItemResult(existingItem.Id, false);
        }
        
        var item = new VocabularyItem(
            addVocabularyItemCommand.Text,
            addVocabularyItemCommand.Language,
            addVocabularyItemCommand.Type);

        await _repository.AddAsync(item, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddVocabularyItemResult(item.Id, true);
    }
}