namespace bow.Application.Users.Register;

public sealed record RegisterUserResult(
    int UserId,
    bool IsCreated
);