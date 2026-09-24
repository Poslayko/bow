namespace bow.Application.UserVocabularyProgresses.Add;

public sealed record AddUserVocabularyProgressCommand(
    int UserId,
    int VocabularyItemId
);