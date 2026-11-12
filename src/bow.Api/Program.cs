using bow.Infrastructure;
using bow.Application;
using bow.Api.Endpoints.ItemVocabulary;
using System.Text.Json.Serialization;
using bow.Api.Endpoints.VocabularyTranslations;
using bow.Api.Common.ExceptionHandling;
using bow.Api.Endpoints.UserVocabularyProgress;
using Telegram.Bot.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

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

builder.Services.AddControllers();
builder.Services.ConfigureTelegramBotMvc();

var app = builder.Build();

app.UseExceptionHandler();

app.MapAddVocabularyItemEndpoint();
app.MapAddVocabularyTranslationEndpoint();
app.MapGetVocabularyTranslationEndpoint();
app.MapAddUserVocabularyProgressEndpoint();

app.MapControllers();

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

public partial class Program;