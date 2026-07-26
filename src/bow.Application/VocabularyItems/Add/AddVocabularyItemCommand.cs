using bow.Domain.Enums;

namespace bow.Application.VocabularyItems.Add;

public sealed record AddVocabularyItemCommand(
    string Text,
    LanguageCode Language,
    VocabularyItemType Type
);