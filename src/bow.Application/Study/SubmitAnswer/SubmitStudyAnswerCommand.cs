namespace bow.Application.Study.SubmitAnswer;

public sealed record SubmitStudyAnswerCommand(
    int UserId,
    int UserVocabularyProgressId,
    string Answer
);