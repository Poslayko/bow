using bow.Domain.Entities;

namespace bow.Application.Users.ConfigureLearning;

public sealed record SendPossibleNativeLanguageCommand(
    User user,
    long telegramId
);