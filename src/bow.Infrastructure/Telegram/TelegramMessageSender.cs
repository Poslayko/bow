using bow.Application.Common.Interfaces;
using bow.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace bow.Infrastructure.Telegram;

public sealed class TelegramMessageSender : ITelegramMessageSender
{
    private readonly ITelegramBotClient _client;

    public TelegramMessageSender(ITelegramBotClient client)
    {
        _client = client;
    }
    public async Task SendWelcomeAsync(
        long telegramId, 
        string? displayName,
        CancellationToken cancellationToken
    )
    {
        string message;
        
        if (displayName is null)
        {
            message = "Hi. Welcome to the chat";
        }
        else
        {
            message = $"Hi, {displayName}. Welcome to the chat";
        }

        await _client.SendMessage(chatId: telegramId, text: message);
    }

    public async Task SendNativeLanguageSelectionAsync(
        long telegramId,
        CancellationToken cancelalationToken
    )
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Russian", "native-language:ru"),
                InlineKeyboardButton.WithCallbackData("Ukrainian", "native-language:ua"),
                InlineKeyboardButton.WithCallbackData("English", "native-language:en"),
                InlineKeyboardButton.WithCallbackData("Nederlands", "native-language:nl")
            }
        });

        await _client.SendMessage(
            chatId: telegramId,
            text: "Choose your native language?",
            replyMarkup: keyboard,
            cancellationToken: cancelalationToken);
    }

    public async Task SendLearningLanguageSelectionAsync(long telegramId, 
        CancellationToken cancelalationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Russian", "learning-language:ru"),
                InlineKeyboardButton.WithCallbackData("Ukrainian", "learning-language:ua"),
                InlineKeyboardButton.WithCallbackData("English", "learning-language:en"),
                InlineKeyboardButton.WithCallbackData("Nederlands", "learning-language:nl")
            }
        });

        await _client.SendMessage(
            chatId: telegramId,
            text: "Choose your learning language?",
            replyMarkup: keyboard,
            cancellationToken: cancelalationToken); 
    }

    public async Task SendErrorLearningAndNativeLanguageCanNotBeTheSame(
        long telegramId,
        CancellationToken cancellationToken
    )
    {
        await _client.SendMessage(chatId: telegramId, text: "Learnig language and native language can't be the same, please choose another language");
    }

    public async Task SendLearningLevelSelectionAsync(long telegramId, 
        CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("A1", "learning-level:A1"),
                InlineKeyboardButton.WithCallbackData("A2", "learning-level:A2"),
                InlineKeyboardButton.WithCallbackData("B1", "learning-level:B1"),
                InlineKeyboardButton.WithCallbackData("B2", "learning-level:B2"),
                InlineKeyboardButton.WithCallbackData("C1", "learning-level:C1"),
                InlineKeyboardButton.WithCallbackData("C2", "learning-level:C2")
            }
        });

        await _client.SendMessage(
            chatId: telegramId,
            text: "Choose what learning level do you want?",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    public async Task ShowMenuHandlerAsync(long telegramId, 
        CancellationToken cancellationToken)
    {
        await _client.SendMessage(
            chatId: telegramId,
            text: "🏠 We are switching to Main Menu",
            replyMarkup: CreateMainMenu(),
            cancellationToken: cancellationToken
        );
    }

    public async Task ShowLearningMenuHandlerAsync(long telegramId, 
        CancellationToken cancellationToken)
    {
        await _client.SendMessage(
            chatId: telegramId,
            text: "📚 We are switching to Learning Menu",
            replyMarkup: AddLearningMenu(),
            cancellationToken: cancellationToken
        );
    }

    public async Task SendMsgUserLearningDataDoesNotSet(long telegramId, 
        CancellationToken cancellationToken)
    {
        await _client.SendMessage(chatId: telegramId, text: "You need to set learning preferences and then you can continue");
    }

    public async Task SendTextMsg(long telegramId, string msg, CancellationToken cancellationToken)
    {
        await _client.SendMessage(telegramId, msg);
    }

    public async Task SendVocabularyItemQuestion(
        long telegramId, 
        string text,
        CancellationToken cancellationToken)
    {
        await _client.SendMessage(telegramId, text);
    }

    private static ReplyKeyboardMarkup CreateMainMenu() => new(new[]
    {
        new[]
        {
            new KeyboardButton("📚 Start learning words")
        },

        new[]
        {
            new KeyboardButton("📊 Show results")
        },

        new[]
        {
            new KeyboardButton("⚙️ Settings")
        }
    })
    {
        ResizeKeyboard = true,
        IsPersistent = true
    };

    private static ReplyKeyboardMarkup AddLearningMenu() => new(new[]
    {
        new[]
        {
            new KeyboardButton("🚫 Stop learning new items")
        }
    })
    {
        ResizeKeyboard = true,
        IsPersistent = true
    };
}