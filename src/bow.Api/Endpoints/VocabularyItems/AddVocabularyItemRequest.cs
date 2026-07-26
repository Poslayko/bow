using bow.Domain.Enums;

namespace bow.Api.Endpoints.ItemVocabulary;

public sealed record AddVocabularyItemRequest(
    string Text,
    LanguageCode Language,
    VocabularyItemType Type
);