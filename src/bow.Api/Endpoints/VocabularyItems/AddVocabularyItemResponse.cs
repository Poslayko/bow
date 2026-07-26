namespace bow.Api.Endpoints.ItemVocabulary;

public sealed record AddVocabularyItemResponse(
    int VocabularyItemId,
    bool IsCreated
);