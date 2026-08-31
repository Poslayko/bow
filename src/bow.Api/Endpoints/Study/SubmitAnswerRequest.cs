namespace bow.Application.Study.SubmitAnswer;

public sealed record SubmitAnswerRequest(
    long TelegramId,
    int UserVocabularyProgressId,
    string Answer
);