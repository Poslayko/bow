namespace bow.Api.Endpoints.UserVocabularyProgress;

public sealed record AddUserVocabularyProgressRequest(
    long TelegramId,
    int VocabularyItemId
);