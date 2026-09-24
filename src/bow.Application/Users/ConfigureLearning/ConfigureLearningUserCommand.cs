using bow.Domain.Enums;

namespace bow.Application.Users.ConfigureLearning;

public sealed record ConfigureLearningUserCommand(
    int UserId,
    LanguageCode NativeLanguage,
    LanguageCode LearningLanguage,
    CefrLevel LearningLevel
);