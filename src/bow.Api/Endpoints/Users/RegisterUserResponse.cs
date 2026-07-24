namespace bow.Api.Endpoints.Users;

public sealed record RegisterUserResponse(
    int UserId,
    bool IsCreated
);