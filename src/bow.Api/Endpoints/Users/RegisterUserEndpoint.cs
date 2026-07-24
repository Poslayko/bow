using bow.Application.Users.Register;

namespace bow.Api.Endpoints.Users;

public static class RegisterUserEndpoint
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "users/register",
            HandleAsync
        );

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequest request,
        RegisterUserHandler handler,
        CancellationToken cancellationToken
    )
    {
        var appCommand = new RegisterUserCommand(request.TelegramId, request.DisplayName);
        var result = await handler.HandleAsync(appCommand, cancellationToken);
        var response = new RegisterUserResponse(
            result.UserId,
            result.IsCreated
        );

        if (result.IsCreated)
        {
            return Results.Created(
                $"/users/{result.UserId}",
                response);
        }

        return Results.Ok(response);
    }
}