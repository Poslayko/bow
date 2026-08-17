using bow.Infrastructure;
using bow.Application;
using bow.Api.Endpoints.Users;
using bow.Api.Endpoints.ItemVocabulary;
using System.Text.Json.Serialization;
using bow.Api.Endpoints.VocabularyTranslations;
using bow.Api.Common.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapRegisterUserEndpoint();
app.MapAddVocabularyItemEndpoint();
app.MapAddVocabularyTranslationEndpoint();
app.MapGetVocabularyTranslationEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
{
    return new {status = "ok"};    
});

app.Run();
