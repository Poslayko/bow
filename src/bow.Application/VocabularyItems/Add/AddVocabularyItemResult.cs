namespace bow.Application.VocabularyItems.Add;

public sealed record AddVocabularyItemResult(
    int VocabularyItemId,
    bool IsCreated
);