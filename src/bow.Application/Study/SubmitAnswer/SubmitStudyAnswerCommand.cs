namespace bow.Application.Study.SubmitAnswer;

public sealed record SubmitStudyAnswerCommand(
    long TelegramId,
    int UserVocabularyProgressId,
    string Answer
);