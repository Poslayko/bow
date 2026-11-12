using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace bow.Api.IntegrationTests.Infrastructure;

public static class TelegramUpdates
{
    public static Update Message(
        string text = "Hello",
        long telegramId = 1234567890,
        string? userName = "Alex")
    {
        return new Update
        {
            Id = 1,
            Message = new Message
            {
                Id = 1,
                From = new User
                {
                    Id = telegramId,
                    IsBot = false,
                    FirstName = "Alex",
                    Username = userName
                },
                Chat = new Chat
                {
                    Id = telegramId,
                    Type = ChatType.Private
                },
                Date = DateTime.UtcNow,
                Text = text
            }
        };
    }

    public static Update MessageWithPressedButton(
        string? callbackData,
        string text = "Press button",
        long telegramId = 1234567890,
        string userName = "Alex"
    )
    {
        return new Update
        {
            Id = 1,
            CallbackQuery = new CallbackQuery
            {
                Id = "1",
                ChatInstance = "test-chat",
                Data = callbackData,
                From = new User
                {
                    Id = telegramId,
                    IsBot = false,
                    FirstName = userName
                },
                Message = new Message
                {
                    Id = 73,
                    Chat = new Chat
                    {
                        Id = telegramId,
                        Type = ChatType.Private
                    },
                    Date = DateTime.UtcNow,
                    Text = text,
                }
            }
        };
    }

    public static Update MessageFromGroup(
        string text = "Hello",
        long telegramId = 1234567890,
        string? userName = "Alex")
    {
        return new Update
        {
            Id = 1,
            Message = new Message
            {
                Id = 1,
                From = new User
                {
                    Id = telegramId,
                    IsBot = false,
                    FirstName = "Alex",
                    Username = userName
                },
                Chat = new Chat
                {
                    Id = telegramId,
                    Type = ChatType.Group
                },
                Date = DateTime.UtcNow,
                Text = text
            }
        };
    }
}