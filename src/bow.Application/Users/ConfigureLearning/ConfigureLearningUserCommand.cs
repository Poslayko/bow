using bow.Domain.Enums;

namespace bow.Application.Users.ConfigureLearning;

public sealed record ConfigureLearningUserCommand(
    long TelegramId,
    LanguageCode NativeLanguage,
    LanguageCode LearningLanguage,
    CefrLevel LearningLevel
);