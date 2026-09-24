namespace bow.Domain.Entities;

public class TelegramAccount
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public long TelegramId { get; private set; }
    public string? Name { get; private set; }
    public User? User { get; private set; }

    public TelegramAccount(long telegramId)
    {
        TelegramId = telegramId;
    }
}