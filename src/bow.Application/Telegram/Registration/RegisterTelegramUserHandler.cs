using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Telegram.Registration;

public sealed class RegisterTelegramUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unit;
    private readonly ITelegramMessageSender _sender;

    public RegisterTelegramUserHandler(IUserRepository userRepository, IUnitOfWork unit,
        ITelegramMessageSender sender)
    {
        _userRepository = userRepository;
        _unit = unit;
        _sender = sender;
    }

    public async Task<User> HandleAsync(
        TelegramInput input,
        CancellationToken cancellationToken
    )
    {
        var user = User.RegisterUserWithTelegramData(input.UserName, input.TelegramUserId);
        
        await _userRepository.AddAsync(user, cancellationToken);

        await _unit.SaveChangesAsync(cancellationToken);

        await _sender.SendWelcomeAsync(input.TelegramUserId, input.UserName, 
            cancellationToken);

        await _unit.SaveChangesAsync(cancellationToken);

        return user;
    }
}