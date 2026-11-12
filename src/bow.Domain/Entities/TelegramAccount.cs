using bow.Domain.Enums;

namespace bow.Domain.Entities;

public class TelegramAccount
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public long TelegramId { get; private set; }
    public string? UserName { get; private set; }
    public TelegramUserState State { get; private set; }
    public User? User { get; private set; }

    public TelegramAccount(long telegramId, string? userName)
    {
        TelegramId = telegramId;
        UserName = userName;
        State = TelegramUserState.Registred;
    }
    
    public void SetState(TelegramUserState state)
    {
        State = state;
    }
}