namespace bow.Application.UserVocabularyProgresses.Add;

public sealed record AddUserVocabularyProgressCommand(
    long TelegramId,
    int VocabularyItemId
);