using bow.Application.VocabularyItems.Add;

namespace bow.Api.Endpoints.ItemVocabulary;

public static class AddVocabularyItemEndpoint
{
    public static IEndpointRouteBuilder MapAddVocabularyItemEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost(
            "/api/v1/vocabulary-items",
            HandleAsync
        );

        return endpoints;
    }

    public static async Task<IResult> HandleAsync(
        AddVocabularyItemRequest request,
        AddVocabularyItemHandler handler,
        CancellationToken token
    )
    {
        if (string.IsNullOrWhiteSpace(request.Text)){
            return Results.BadRequest();
        }

        var addItemCommand = new AddVocabularyItemCommand(request.Text,
            request.Language, request.Type);

        var result = await handler.HandleAsync(addItemCommand, token);

        var response = new AddVocabularyItemResponse(result.VocabularyItemId,
            result.IsCreated);

        if (result.IsCreated)
        {
            return Results.Created(
                $"/api/v1/vocabulary-items/{response.VocabularyItemId}",
                response);
        }

        return Results.Ok(response);
    }
}