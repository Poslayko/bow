using bow.Domain.Enums;

namespace bow.Api.Endpoints.Users;

public sealed record ConfigureLearningUserRequest(
    LanguageCode NativeLanguage,
    LanguageCode LearningLanguage,
    CefrLevel LearningLevel
);