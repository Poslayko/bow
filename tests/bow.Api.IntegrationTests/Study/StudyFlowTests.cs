using bow.Api.IntegrationTests.Infrastructure;
using bow.Application.Telegram.Study.StartNext;
using bow.Domain.Entities;
using bow.Domain.Enums;
using bow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace bow.Api.IntegrationTests.Study;

public sealed class StudyFlowTests : IClassFixture<PostgresFixture>
{
    private readonly HttpClient _client;
    private readonly BowApiFactory _factory;
    private readonly FakeTelegramMessageSender _sender;

    public StudyFlowTests(PostgresFixture postgres)
    {
        _factory = new BowApiFactory(postgres.ConnectionString);
        _client = _factory.CreateClient();
        _sender = _factory.TelegramSender;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static Task<UserVocabularyProgress> GetProgressAsync(
        AppDbContext db,
        int userId,
        UserVocabularyProgressStatus status
    )
    {
        return db.UserVocabularyProgresses
            .Include(x => x.VocabularyItem)
            .SingleAsync(x =>
                x.UserId == userId
                && x.Status == status);
    }

    [Fact]
    public async Task SendUpdate_WithoutSecretKey_ReturnUnauthorized()
    {
        using var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        var update = new Update
        {
            Id = 123456789,
            Message = new Message
            {
                Id = 42,
                From = new Telegram.Bot.Types.User
                {
                    Id = 1234567890,
                    IsBot = false,
                    FirstName = "Alex",
                    Username = "alex",
                    LanguageCode = "ru"
                },
                Chat = new Chat
                {
                    Id = 1223334444,
                    Type = ChatType.Private,
                    FirstName = "Alex",
                    Username = "alex"
                },
                Date = DateTime.UtcNow,
                Text = "Hello"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(await db.Users.AnyAsync(
            x => x.TelegramAccount != null 
            && x.TelegramAccount.TelegramId == update.Message.From.Id
        ));
        Assert.Empty(_sender.Messages);
    }

    [Fact]
    public async Task SendFirstMessage_CreateUser_GetWelcomeMsgAndNativeLanguageSelection()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        string firstMsg = "First msg";
        long telegramId = 1234567890;
        string userName = "Alex";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);

        Assert.True(await db.Users.AnyAsync(user => user.TelegramAccount != null
            && user.TelegramAccount.UserName == userName));
        Assert.Equal($"SendWelcomeAsync:{userName}", _sender.Messages[0].Text);
        Assert.Equal(telegramId, _sender.Messages[0].TelegramId);
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.TelegramAccounts.CountAsync());
        Assert.Equal("SendNativeLanguageSelectionAsync", _sender.Messages[1].Text);
        Assert.Equal(telegramId, _sender.Messages[1].TelegramId);
    }

    [Fact]
    public async Task SendStartTwoTimes_CreateUser_GetWelcomeMsgOneTimeAndNativeLanguageSelectionTwoTimes()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Alex";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        var response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);

        Assert.True(await db.Users.AnyAsync(user => user.TelegramAccount != null
            && user.TelegramAccount.UserName == userName));
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.TelegramAccounts.CountAsync());
        Assert.Equal(3, _sender.Messages.Count());
        Assert.Equal("SendNativeLanguageSelectionAsync", _sender.Messages[1].Text);
        Assert.Equal("SendNativeLanguageSelectionAsync", _sender.Messages[2].Text);
    }

    [Fact]
    public async Task SettingLearningProfile_WhitoutExceptions_SetLearningProfile()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:B1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);
        Assert.Equal(LanguageCode.Ua, telegramAccount.User.NativeLanguage);
        Assert.Equal(LanguageCode.Nl, telegramAccount.User.LearningLanguage);
        Assert.Equal(CefrLevel.B1, telegramAccount.User.LearningLevel);
        Assert.Equal(TelegramUserState.ChoosingInMenu, telegramAccount.State);
        Assert.Equal(5, _sender.Messages.Count());
        Assert.Equal($"SendWelcomeAsync:{userName}", _sender.Messages[0].Text);
        Assert.Equal("SendNativeLanguageSelectionAsync", _sender.Messages[1].Text);
        Assert.Equal("SendLearningLanguageSelectionAsync", _sender.Messages[2].Text);
        Assert.Equal("SendLearningLevelSelectionAsync", _sender.Messages[3].Text);
        Assert.Equal("ShowMenuHandlerAsync", _sender.Messages[4].Text);
    }

    [Fact]
    public async Task SettingLearningLanguage_InvalidCallbackQuery_ResendLearningSelection()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:fr"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage); 

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);
        Assert.Equal(LanguageCode.Ua, telegramAccount.User.NativeLanguage);
        Assert.Null(telegramAccount.User.LearningLanguage);
        Assert.Equal(TelegramUserState.ChoosingLearningLanguage, telegramAccount.State);
        Assert.Equal(3, _sender.Messages.Count());
        Assert.Equal($"SendWelcomeAsync:{userName}", _sender.Messages[0].Text);
        Assert.Equal("SendNativeLanguageSelectionAsync", _sender.Messages[1].Text);
        Assert.Equal("SendLearningLanguageSelectionAsync", _sender.Messages[2].Text);
    }

    [Fact]
    public async Task StartLearningProcess_WithNewAccount_OneProcessInWaitingStatus()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:B1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);
        Assert.Equal(7, _sender.Messages.Count());

        Assert.Equal(LanguageCode.Ua, telegramAccount.User.NativeLanguage);
        Assert.Equal(LanguageCode.Nl, telegramAccount.User.LearningLanguage);
        Assert.Equal(CefrLevel.B1, telegramAccount.User.LearningLevel);
        
        Assert.Equal("ShowLearningMenuHandlerAsync", _sender.Messages[5].Text);
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);

        var userVocabularyProcess = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == telegramAccount.UserId);

        Assert.NotNull(userVocabularyProcess);
        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, userVocabularyProcess.Status);
        Assert.Equal(_sender.Messages[6].Text, userVocabularyProcess.VocabularyItem.Text);
    }

    [Fact]
    public async Task StartLearning_WhenYouAlreadyHaveProcess_CheckAllStates()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:B1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);

        var userVocabularyProcess = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == telegramAccount.UserId);

        var originalNextReviewAt = userVocabularyProcess.NextReviewAt;

        Assert.NotNull(userVocabularyProcess);
        Assert.Null(userVocabularyProcess.LastReviewedAt);
        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, userVocabularyProcess.Status);
        Assert.Equal(_sender.Messages[6].Text, userVocabularyProcess.VocabularyItem.Text); 
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);

        var updateWithStopLearning = TelegramUpdates.Message("🚫 Stop learning new items");

        var _response6 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStopLearning);

        await db.Entry(telegramAccount).ReloadAsync();

        Assert.Equal(TelegramUserState.ChoosingInMenu, telegramAccount.State);

        var _response7 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var userVocabularyProgresses = await db.UserVocabularyProgresses
            .Where(x => x.UserId == telegramAccount.UserId)
            .ToListAsync();

        await db.Entry(telegramAccount).ReloadAsync();

        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);

        Assert.NotNull(userVocabularyProgresses);
        Assert.Single(userVocabularyProgresses);
        Assert.Equal(userVocabularyProcess.VocabularyItem.Text, _sender.Messages[6].Text);
        Assert.Equal(userVocabularyProcess.Id, userVocabularyProgresses[0].Id);
        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, userVocabularyProcess.Status);
        Assert.Equal(LearningStage.NotStarted, userVocabularyProgresses[0].Stage);
        Assert.Equal(originalNextReviewAt, userVocabularyProgresses[0].NextReviewAt);
    }

    [Fact]
    public async Task TelegramStudy_CorrectAnswer_UpdatesProgress()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:B1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);

        var userVocabularyProcess = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == telegramAccount.UserId);

        Assert.NotNull(userVocabularyProcess);
        Assert.Equal(LearningStage.NotStarted, userVocabularyProcess.Stage);
        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, userVocabularyProcess.Status);

        var correctAnswer = await db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProcess.VocabularyItemId
                && x.TranslationTo.Language == LanguageCode.Ua)
            .Select(x => x.TranslationTo.Text)
            .FirstAsync();
        
        var updateWithCorrectAnswer = TelegramUpdates.Message(correctAnswer, 
            telegramId, userName);

        var _response6 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCorrectAnswer);

        await db.Entry(userVocabularyProcess).ReloadAsync();
        await db.Entry(telegramAccount).ReloadAsync();

        var translations = await db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProcess.VocabularyItemId
                && x.TranslationTo.Language == telegramAccount.User.NativeLanguage)
            .Select(x => x.TranslationTo.Text)
            .ToListAsync();

        var correctAnswerResponse = $"Your answer: {correctAnswer} was ✅\n possible translation: {string.Join(", ", translations)}";
        
        Assert.Equal(_sender.Messages[7].Text, correctAnswerResponse);
        Assert.Equal(LearningStage.FirstCorrect, userVocabularyProcess.Stage);
        Assert.Equal(UserVocabularyProgressStatus.idle, userVocabularyProcess.Status);
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);

        var newUserVocabularyProcess = await db.UserVocabularyProgresses
            .SingleAsync(x => 
                x.UserId == telegramAccount.UserId 
                && x.Status == UserVocabularyProgressStatus.waiting_answer);

        Assert.NotEqual(userVocabularyProcess.VocabularyItemId, newUserVocabularyProcess.VocabularyItemId);
        Assert.Equal(newUserVocabularyProcess.VocabularyItem.Text, _sender.Messages[8].Text);
    }

    [Fact]
    public async Task TelegramStudy_WrongAnser_UpdatesProgressAndCheckErrorMsgs()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:B1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);

        var userVocabularyProcess = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == telegramAccount.UserId);

        userVocabularyProcess.SetStage(LearningStage.After1Day);
        await db.SaveChangesAsync();

        var correctAnswer = await db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProcess.VocabularyItemId
                && x.TranslationTo.Language == LanguageCode.Ua)
            .Select(x => x.TranslationTo.Text)
            .FirstAsync();
        
        var wrongAnswer = "error";
        
        var updateWithWrongAnswer = TelegramUpdates.Message(wrongAnswer, 
            telegramId, userName);

        var _response6 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithWrongAnswer);

        await db.Entry(telegramAccount).ReloadAsync();

        var translations = await db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProcess.VocabularyItemId
                && x.TranslationTo.Language == telegramAccount.User.NativeLanguage)
            .Select(x => x.TranslationTo.Text)
            .ToListAsync();

        var wrongAnswerResponse = $"Your answer: {wrongAnswer} was ❌\npossible translation: {string.Join(", ", translations)}";

        var newUserVocabularyProcess = await db.UserVocabularyProgresses
            .AsNoTracking()
            .Include(x => x.VocabularyItem)
            .SingleAsync(x => 
                x.UserId == telegramAccount.UserId
                && x.Status == UserVocabularyProgressStatus.waiting_answer);
        
        Assert.Equal(wrongAnswerResponse, _sender.Messages[7].Text);
        Assert.Equal(LearningStage.NotStarted, newUserVocabularyProcess.Stage);
        Assert.Equal(userVocabularyProcess.Id, newUserVocabularyProcess.Id);
        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, newUserVocabularyProcess.Status);
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);
        Assert.Equal(newUserVocabularyProcess.VocabularyItem.Text, _sender.Messages[8].Text);
    }

    [Fact]
    public async Task TelegramStudy_TwoUsers_ProgresIsIsolated()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var firstUser = await db.Users
            .Include(x => x.TelegramAccount)
            .SingleAsync(x => x.TelegramAccount!.TelegramId == telegramId);

        Assert.NotNull(firstUser.TelegramAccount);
        firstUser.SetNativeLanguage(LanguageCode.Ua);
        firstUser.SetLearningLanguage(LanguageCode.Nl);
        firstUser.SetLearningLevel(CefrLevel.A1);
        firstUser.TelegramAccount.SetState(TelegramUserState.ChoosingInMenu);

        await db.SaveChangesAsync();

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithStartLearning);

        var firstUserReloaded = await db.Users
            .AsNoTracking()
            .Include(x => x.TelegramAccount)
            .SingleAsync(x => x.Id == firstUser.Id);
        var userVocabularyProgress = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == firstUser.Id);

        Assert.NotNull(firstUserReloaded.TelegramAccount);
        var firstUserTelegramState = firstUserReloaded.TelegramAccount.State;
        var userVocabularyProgressId = userVocabularyProgress.Id;
        var userVocabularyProgressStage = userVocabularyProgress.Stage;
        var userVocabularyProgressLastReviewed = userVocabularyProgress.LastReviewedAt;
        var userVocabularyProgressNextReview = userVocabularyProgress.NextReviewAt;
        var userVocabularyProgressStatus = userVocabularyProgress.Status;

        var secondUserTelegramId = 12345;
        var secondUserName = "Anatoly";
        var updateSecondUser = TelegramUpdates.Message(firstMsg, secondUserTelegramId,
            secondUserName);

        var _responseSecondUser = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateSecondUser);

        var secondUser = await db.Users
            .Include(x => x.TelegramAccount)
            .SingleAsync(x => x.TelegramAccount!.TelegramId == secondUserTelegramId);

        Assert.NotNull(secondUser.TelegramAccount);
        secondUser.SetNativeLanguage(LanguageCode.Ua);
        secondUser.SetLearningLanguage(LanguageCode.Nl);
        secondUser.SetLearningLevel(CefrLevel.B1);
        secondUser.TelegramAccount.SetState(TelegramUserState.ChoosingInMenu);

        await db.SaveChangesAsync();

        var updateWithStartLearning2 = TelegramUpdates.Message("📚 Start learning words", 
            secondUserTelegramId, secondUserName);

        var _responseSecondUser_2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning2);

        var userVocabularyProgress_2 = await db.UserVocabularyProgresses
            .SingleAsync(x => x.UserId == secondUser.Id);

        Assert.NotNull(userVocabularyProgress);
        Assert.NotNull(userVocabularyProgress_2);

        var correctAnswer_2 = await db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProgress_2.VocabularyItemId
                && x.TranslationTo.Language == LanguageCode.Ua)
            .Select(x => x.TranslationTo.Text)
            .FirstAsync();
        
        var updateWithCorrectAnswer_2 = TelegramUpdates.Message(correctAnswer_2, 
            secondUserTelegramId, secondUserName);

        var _response3_2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCorrectAnswer_2);

        await db.Entry(userVocabularyProgress_2).ReloadAsync();

        var userVocabularyProgressReloaded = await db.UserVocabularyProgresses
            .AsNoTracking()
            .SingleAsync(x => x.Id == userVocabularyProgress.Id);

        var firstUserReloaded2 = await db.Users
            .AsNoTracking()
            .Include(x => x.TelegramAccount)
            .SingleAsync(x => x.Id == firstUser.Id);

        Assert.Equal(UserVocabularyProgressStatus.idle, userVocabularyProgress_2.Status); 
        Assert.Equal(LearningStage.FirstCorrect, userVocabularyProgress_2.Stage);
        Assert.NotNull(userVocabularyProgress_2.LastReviewedAt);
        Assert.NotNull(firstUserReloaded2.TelegramAccount);
        Assert.Equal(firstUserTelegramState, 
            firstUserReloaded2.TelegramAccount.State);

        Assert.Equal(userVocabularyProgressId,
            userVocabularyProgressReloaded.Id);
        Assert.Equal(userVocabularyProgressStatus, 
            userVocabularyProgressReloaded.Status);
        Assert.Equal(userVocabularyProgressStage, userVocabularyProgressReloaded.Stage);
        Assert.Equal(userVocabularyProgressLastReviewed, 
            userVocabularyProgressReloaded.LastReviewedAt);
        Assert.Equal(userVocabularyProgressNextReview,
            userVocabularyProgressReloaded.NextReviewAt);
    }

    [Fact]
    public async Task TelegramOnboarding_StaleCallback_DoesNotChangeProfile()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        Assert.Equal("SendLearningLevelSelectionAsync", _sender.Messages[3].Text);

        var updateWithLearningLanguage2 = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:en"
        );

        
        var user = await db.Users
            .Include(x => x.TelegramAccount)
            .SingleAsync(x => x.TelegramAccount!.TelegramId == telegramId);

        Assert.Equal(LanguageCode.Nl, user.LearningLanguage);
        Assert.NotNull(user.TelegramAccount);

        var currentTelegramState = user.TelegramAccount.State;

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithLearningLanguage2);

        await db.Entry(user).ReloadAsync();

        Assert.Equal(LanguageCode.Nl, user.LearningLanguage);        
        Assert.Equal(currentTelegramState, user.TelegramAccount.State);
    }

    [Fact]
    public async Task TelegramStudy_NoAvailableItems_ReturnsToMenu()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:A1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();
        
        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);

        var userVocabularyProgress = await GetProgressAsync(db, telegramAccount.UserId, UserVocabularyProgressStatus.waiting_answer); 

        var getCorrectAnswer = db.VocabularyTranslations
            .Where(x => 
                x.TranslationFromId == userVocabularyProgress.VocabularyItemId
                && x.TranslationTo.Language == LanguageCode.Ua)
            .Select(x => x.TranslationTo.Text)
            .FirstAsync();

        var correctAnswer = await getCorrectAnswer;
        
        var updateWithCorrectAnswer = TelegramUpdates.Message(correctAnswer, 
            telegramId, userName);

        var _response6 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCorrectAnswer);

        var _startNextStudyItemHandler = TestDatabase.GetService<StartNextStudyItemHandler>(
            scope);

        await _startNextStudyItemHandler.HandleAsync(telegramAccount.User, telegramAccount,
            telegramAccount.TelegramId, CancellationToken.None);

        var lastReviewedAt = DateTime.UtcNow;
        var nextReviewAt = DateTime.UtcNow.AddHours(1);

        var secondVocabularyItem = await GetProgressAsync(db, telegramAccount.UserId, 
            UserVocabularyProgressStatus.waiting_answer);

        secondVocabularyItem.SetStage(LearningStage.FirstCorrect);
        secondVocabularyItem.SetLastReviewedAt(lastReviewedAt);
        secondVocabularyItem.SetNextReviewAt(nextReviewAt);
        secondVocabularyItem.SetStatus(UserVocabularyProgressStatus.idle);

        await db.SaveChangesAsync();

        await _startNextStudyItemHandler.HandleAsync(telegramAccount.User, telegramAccount,
            telegramAccount.TelegramId, CancellationToken.None);

        var thirdVocabularyItem = await GetProgressAsync(db, telegramAccount.UserId, 
            UserVocabularyProgressStatus.waiting_answer);

        thirdVocabularyItem.SetStage(LearningStage.FirstCorrect);
        thirdVocabularyItem.SetLastReviewedAt(lastReviewedAt);
        thirdVocabularyItem.SetNextReviewAt(nextReviewAt);
        thirdVocabularyItem.SetStatus(UserVocabularyProgressStatus.idle);

        await db.SaveChangesAsync();

        await _startNextStudyItemHandler.HandleAsync(telegramAccount.User, telegramAccount,
            telegramAccount.TelegramId, CancellationToken.None);

        await db.Entry(telegramAccount).ReloadAsync();

        var newWaitingProgress = await db.UserVocabularyProgresses
            .SingleOrDefaultAsync(x => 
                x.UserId == telegramAccount.UserId
                && x.Status == UserVocabularyProgressStatus.waiting_answer
            );

        Assert.Equal(TelegramUserState.ChoosingInMenu, telegramAccount.State);
        Assert.Equal("You don't have new words for learning", _sender.Messages[11].Text);
        Assert.Null(newWaitingProgress);
    }

    [Fact]
    public async Task Telegram_UnsupportedUpdate_IsIgnored()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.MessageFromGroup(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);

        var users = await db.Users
            .Include(x => x.TelegramAccount)
            .ToListAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task TelegramStudy_AnswerWithoutWaitingProgress_RecoversWithoutChangingSrs()
    {
        var scope = _factory.Services.CreateScope();

        await TestDatabase.ResetTables(scope);

        var db = TestDatabase.GetService<AppDbContext>(scope);

        await TestVocabulary.SeedAsync(db);

        string firstMsg = "/start";
        long telegramId = 1234567890;
        string userName = "Fedr";
        var update = TelegramUpdates.Message(firstMsg, telegramId, userName); 

        _client.DefaultRequestHeaders.Add(
            "X-Telegram-Bot-Api-Secret-Token", 
            BowApiFactory.WebhookSecret);

        var _response = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", update);
        
        var updateWithNativeLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "native-language:ua"
        );

        var _response2 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithNativeLanguage);
        
        var updateWithLearningLanguage = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-language:nl"
        );

        var _response3 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook", 
            updateWithLearningLanguage);

        var updateWithCefrLevel = TelegramUpdates.MessageWithPressedButton(
            userName: userName, telegramId: telegramId, callbackData: "learning-level:A1"
        );

        var _response4 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCefrLevel);

        var updateWithStartLearning = TelegramUpdates.Message("📚 Start learning words", 
            telegramId, userName);

        var _response5 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithStartLearning);

        var telegramAccount = await db.TelegramAccounts
            .AsNoTracking()
            .Where(telegarmAccount => telegarmAccount.TelegramId == telegramId)
            .Include(TelegramAccount => TelegramAccount.User)
            .SingleOrDefaultAsync();

        Assert.NotNull(telegramAccount);
        Assert.NotNull(telegramAccount.User);

        var vocabularyItemText = _sender.Messages[6].Text;

        Assert.NotNull(vocabularyItemText);

        var vocabularyProgress = await db.UserVocabularyProgresses
            .Where(x => 
                x.UserId == telegramAccount.User.Id
                && x.VocabularyItem.Text == vocabularyItemText)
            .SingleAsync();

        vocabularyProgress.SetStatus(UserVocabularyProgressStatus.idle);

        await db.SaveChangesAsync();

        await db.Entry(vocabularyProgress).ReloadAsync();

        Assert.Equal(UserVocabularyProgressStatus.idle, vocabularyProgress.Status);
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccount.State);
        
        var translation = await db.VocabularyTranslations
            .Include(x => x.TranslationTo)
            .SingleAsync(x => x.TranslationFromId == vocabularyProgress.VocabularyItemId);

        var updateWithCorrectAnswer = TelegramUpdates.Message(
            translation.TranslationTo.Text,
            telegramId,
            userName
        );

        var _response6 = await _client.PostAsJsonAsync("/api/v1/telegram/webhook",
            updateWithCorrectAnswer);

        Assert.Equal("Something went wrong, waiting vocabulary item wasn't found",
            _sender.Messages[7].Text);

        using var verificationScope = _factory.Services.CreateScope();
        var db2 = TestDatabase.GetService<AppDbContext>(verificationScope);

        var vocabularyProgressReloaded = await db2.UserVocabularyProgresses
            .Where(x =>
                x.UserId == telegramAccount.User.Id
                && x.VocabularyItem.Text == vocabularyItemText)
            .SingleAsync();

        var telegramAccountReloaded = await db2.TelegramAccounts
            .Where(x => x.TelegramId == telegramId)
            .SingleAsync();

        Assert.Equal(UserVocabularyProgressStatus.waiting_answer, 
            vocabularyProgressReloaded.Status);
        Assert.Equal(LearningStage.NotStarted, vocabularyProgressReloaded.Stage);
        Assert.Equal(TelegramUserState.SubmitAnswer, telegramAccountReloaded.State);
        Assert.Equal(vocabularyProgress.LastReviewedAt, 
            vocabularyProgressReloaded.LastReviewedAt);
        Assert.Equal(vocabularyProgress.NextReviewAt, 
            vocabularyProgressReloaded.NextReviewAt);
        Assert.Equal(1, 
            await db2.UserVocabularyProgresses.CountAsync(x => 
                x.UserId == telegramAccountReloaded.UserId));
        Assert.Equal(vocabularyItemText, _sender.Messages[8].Text);
    }
} 