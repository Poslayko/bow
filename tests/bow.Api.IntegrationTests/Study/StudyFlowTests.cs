using System.Net;
using System.Net.Http.Json;
using bow.Api.IntegrationTests.Infrastructure;
using bow.Domain.Enums;
using bow.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using bow.Api.Endpoints.Study;
using System.Text.Json;
using System.Text.Json.Serialization;
using bow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bow.Api.IntegrationTests.Study;

public sealed class StudyFlowTests : IClassFixture<PostgresFixture>
{
    private readonly HttpClient _client;
    private readonly BowApiFactory _factory;

    public StudyFlowTests(PostgresFixture postgres)
    {
        _factory = new BowApiFactory(postgres.ConnectionString);
        _client = _factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetStudyNext_WhenNoData_ReturnNoContent()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);

        await TestDataFactory.ConfigureLearningAsync(user, unit);

        Assert.NotNull(user.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetStudyNext_WhenWeHaveAvailableProgress_ReturnUserProgress()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var db = TestDatabase.GetService<AppDbContext>(scope);

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);

        await TestDataFactory.ConfigureLearningAsync(user, unit);

        var itemEn = await TestDataFactory.CreateItemAsync(items, unit, "Apple", 
            LanguageCode.En);
        var itemRu = await TestDataFactory.CreateItemAsync(items, unit, "Яблоко", 
            LanguageCode.Ru);

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemEn, itemRu);

        Assert.NotNull(user.TelegramAccount);
        
        var response = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });
    
        var responseBody = await response.Content
            .ReadFromJsonAsync<StartNextStudyItemResponse>(JsonOptions);

        Assert.NotNull(responseBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(itemEn.Text, responseBody.Text);
        Assert.Equal(LearningStage.NotStarted, responseBody.Stage);

        var countedPorgresses = await db.UserVocabularyProgresses
            .Where(progress => progress.UserId == user.Id)
            .CountAsync();

        Assert.Equal(1, countedPorgresses);

        var responseFromSecondTry = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });

        var responseFromSecondTryBody = await responseFromSecondTry.Content
            .ReadFromJsonAsync<StartNextStudyItemResponse>(JsonOptions);

        var countedPorgressesAtSecondTime = await db.UserVocabularyProgresses
            .Where(progress => progress.UserId == user.Id)
            .CountAsync();

        Assert.NotNull(responseFromSecondTryBody);

        Assert.Equal(responseBody.UserVocabularyProgressId, 
            responseFromSecondTryBody.UserVocabularyProgressId);
        Assert.Equal(countedPorgresses, countedPorgressesAtSecondTime);
    }

    [Fact]
    public async Task GetStudyNext_WhenWeHaveOneAvailableProgressAndOnePossible_ReturnOneAvailable()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var progresses = TestDatabase.GetService<IUserVocabularyProgressRepository>(scope);
        var db = TestDatabase.GetService<AppDbContext>(scope);

        var itemThatWillBeInProgress = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "ReAd",
            LanguageCode.En
        );

        var itemThatWillBeInProgressTranslation = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "ЧиТаТь",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(
            translations,
            unit,
            itemThatWillBeInProgress,
            itemThatWillBeInProgressTranslation
        );
        
        var itemFree = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "WrIte",
            LanguageCode.En
        );

        var itemFreeTranslation = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "пИсать",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(
            translations,
            unit, 
            itemFree,
            itemFreeTranslation
        );

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);

        await TestDataFactory.ConfigureLearningAsync(user, unit);

        var now = DateTime.UtcNow;

        var progress = await TestDataFactory.CreateProgressAsync(progresses, unit, user.Id,
            itemThatWillBeInProgress.Id, now);
        
        Assert.NotNull(user.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });        

        var responseBody = await response.Content
            .ReadFromJsonAsync<StartNextStudyItemResponse>(JsonOptions);
        
        var progressCount = await db.UserVocabularyProgresses
            .Where(progress => progress.UserId == user.Id)
            .CountAsync();

        Assert.Equal(1, progressCount);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(itemThatWillBeInProgress.Text, responseBody.Text);
    }

    [Fact]
    public async Task GetStudyNext_WhenWeHaveDifferentItems_GetOnlyItemsForCurrentLevel()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var users = TestDatabase.GetService<IUserRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var db = TestDatabase.GetService<AppDbContext>(scope);

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);
        await TestDataFactory.ConfigureLearningAsync(user, unit, LanguageCode.Ru, 
            LanguageCode.En, CefrLevel.B1);

        var itemWithHighLevelEn = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "Apple",
            LanguageCode.En
        );

        var itemWithHighLevelRu = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "яблоКо",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemWithHighLevelEn, itemWithHighLevelRu, CefrLevel.B2);

        var itemWithWrongLanguageEn = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "RuN",
            LanguageCode.En
        );

        var itemWithWrongLanguageNl = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "lOOp",
            LanguageCode.Nl
        );

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemWithWrongLanguageEn, itemWithWrongLanguageNl, CefrLevel.A1);

        var itemNl = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "Ik",
            LanguageCode.Nl
        );

        var itemNlRu = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "Я",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemNl, itemNlRu, CefrLevel.A1);

        var itemEn = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "write",
            LanguageCode.En
        );

        var itemRu = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "писать",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemEn, itemRu, CefrLevel.A2);

        Assert.NotNull(user.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });

        var countedPorgresses = await db.UserVocabularyProgresses
            .Where(progress => progress.UserId == user.Id)
            .CountAsync();

        Assert.Equal(1, countedPorgresses);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content
            .ReadFromJsonAsync<StartNextStudyItemResponse>(JsonOptions);

        Assert.Equal(itemEn.Text, responseBody?.Text);
    }

    [Fact]
    public async Task SubmitAnswer_WhenAllIsFine_ReturnCorrectData()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var progresses = TestDatabase.GetService<IUserVocabularyProgressRepository>(scope);

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);

        user.ConfigureLearning(LanguageCode.Ru, LanguageCode.En, CefrLevel.B1);
        await unit.SaveChangesAsync(CancellationToken.None);

        var itemEn = await TestDataFactory.CreateItemAsync(
            items, 
            unit, 
            "apple",
            LanguageCode.En
        );

        var itemRu = await TestDataFactory.CreateItemAsync(
            items,
            unit,
            "яблоко",
            LanguageCode.Ru
        );

        await TestDataFactory.CreateTranslationAsync(translations, unit, itemEn, 
            itemRu, CefrLevel.A1);

        var now = DateTime.UtcNow;
        var progress = await TestDataFactory.CreateProgressAsync(progresses, unit,
            user.Id, itemEn.Id, now);

        Assert.NotNull(user.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/answer", new
        {
            TelegramId = user.TelegramAccount.TelegramId,
            UserVocabularyProgressId = progress.Id,
            Answer = " ЯБЛОКО "
        });

        var body = await response.Content
            .ReadFromJsonAsync<SubmitAnswerResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body?.IsCorrect);
        Assert.Equal(LearningStage.NotStarted, body?.PreviousStage);
        Assert.Equal(LearningStage.FirstCorrect, body?.CurrentStage);
        Assert.Equal(["яблоко"], body?.AcceptedAnswers);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        var currentProgress = await db.UserVocabularyProgresses
            .AsNoTracking()
            .SingleOrDefaultAsync(curProgress => curProgress.Id == progress.Id);

        Assert.Equal(LearningStage.FirstCorrect, currentProgress?.Stage);
        Assert.NotNull(currentProgress?.LastReviewedAt);
        Assert.True(currentProgress?.NextReviewAt > currentProgress?.LastReviewedAt);

        var responseStartStudyNext = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });

        Assert.Equal(HttpStatusCode.NoContent, responseStartStudyNext.StatusCode);
    }

    [Fact]
    public async Task SubmitAnswer_WithWrongAnswer_ReturnResponseWithNotStarted()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var progresses = TestDatabase.GetService<IUserVocabularyProgressRepository>(scope);
        var db = TestDatabase.GetService<AppDbContext>(scope);

        var user = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);

        await TestDataFactory.ConfigureLearningAsync(user, unit, LanguageCode.Ru,
            LanguageCode.En, CefrLevel.B1);
        await unit.SaveChangesAsync(CancellationToken.None);

        var itemEn = await TestDataFactory.CreateItemAsync(items, unit,
            "apple", LanguageCode.En);

        var itemRu = await TestDataFactory.CreateItemAsync(items, unit,
            "яблоко", LanguageCode.Ru);

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemEn, itemRu, CefrLevel.A1);
        
        var initialTime = DateTime.UtcNow.AddHours(-10);
        var progress = await TestDataFactory.CreateProgressAsync(progresses, unit,
            user.Id, itemEn.Id, initialTime);

        progress.SetStage(LearningStage.After3Hours);
        progress.SetLastReviewedAt(initialTime.AddHours(-3));
        await unit.SaveChangesAsync(CancellationToken.None);

        var beforeRequest = DateTime.UtcNow;

        Assert.NotNull(user.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/answer", new
        {
            TelegramId = user.TelegramAccount.TelegramId,
            UserVocabularyProgressId = progress.Id,
            Answer = "банан"
        });

        var afterRequest = DateTime.UtcNow;

        var body = await response.Content
            .ReadFromJsonAsync<SubmitAnswerResponse>(JsonOptions);

        Assert.False(body?.IsCorrect);
        Assert.Equal(LearningStage.NotStarted, body?.CurrentStage);

        var currentProgress = await db.UserVocabularyProgresses
            .AsNoTracking()
            .SingleAsync(x => x.Id == progress.Id);

        Assert.Equal(LearningStage.NotStarted, currentProgress.Stage);
        Assert.True(
            beforeRequest <= currentProgress.NextReviewAt &&
            currentProgress.NextReviewAt <= afterRequest);

        var responseGetNext = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = user.TelegramAccount.TelegramId
        });

        var bodyGetNext = await responseGetNext.Content
            .ReadFromJsonAsync<StartNextStudyItemResponse>(JsonOptions);

        Assert.Equal(progress.Id, bodyGetNext?.UserVocabularyProgressId);
        Assert.Equal(LearningStage.NotStarted, bodyGetNext?.Stage);
    }

    [Fact]
    public async Task CheckingSubmitAnswer_WithProgressThatDidNotBelongToUser_ResponseNotFound()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var users = TestDatabase.GetService<IUserRepository>(scope);
        var unit = TestDatabase.GetService<IUnitOfWork>(scope);
        var items = TestDatabase.GetService<IVocabularyItemRepository>(scope);
        var translations = TestDatabase.GetService<IVocabularyTranslationRepository>(scope);
        var progresses = TestDatabase.GetService<IUserVocabularyProgressRepository>(scope);

        var userWithOneProgress = await TestDataFactory.CreateUserAsync(users, unit, 
            name: "Alex", telegramId: 1234567890);
        var userWithoutProgresses = await TestDataFactory.CreateUserAsync(users, unit,
            name: "Alex", telegramId: 0987654321);

        await TestDataFactory.ConfigureLearningAsync(userWithOneProgress, unit,
            LanguageCode.Ru, LanguageCode.En, CefrLevel.B1);
        await TestDataFactory.ConfigureLearningAsync(userWithoutProgresses, unit, 
            LanguageCode.Ru, LanguageCode.En, CefrLevel.B1);

        await unit.SaveChangesAsync(CancellationToken.None);

        var itemEn = await TestDataFactory.CreateItemAsync(items, unit, "apple", LanguageCode.En);
        var itemRu = await TestDataFactory.CreateItemAsync(items, unit, "яблоко", LanguageCode.Ru);

        await TestDataFactory.CreateTranslationAsync(translations, unit,
            itemEn, itemRu, CefrLevel.A1);

        var progress = await TestDataFactory.CreateProgressAsync(progresses, unit, 
            userWithOneProgress.Id, itemEn.Id, DateTime.UtcNow);

        Assert.NotNull(userWithoutProgresses.TelegramAccount);

        var response = await _client.PostAsJsonAsync("/api/v1/study/answer", new
        {
            TelegramId = userWithoutProgresses.TelegramAccount.TelegramId,
            UserVocabularyProgressId = progress.Id,
            Answer = "яблоко"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartNext_ForTelegramUserIdThatDoNotExist_ResultNotFound()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var response = await _client.PostAsJsonAsync("/api/v1/study/next", new
        {
            TelegramId = 1234567890
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}