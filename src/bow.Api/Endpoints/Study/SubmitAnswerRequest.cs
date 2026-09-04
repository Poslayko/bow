namespace bow.Api.Endpoints.Study;

public sealed record SubmitAnswerRequest(
    long TelegramId,
    int UserVocabularyProgressId,
    string Answer
);