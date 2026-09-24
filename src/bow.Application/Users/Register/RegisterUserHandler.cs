using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Users.Register;

public sealed class RegisterUserHandler
{
    private readonly ITelegramAccountRepository _telegramAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(ITelegramAccountRepository telegramAccountRepository, 
        IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _telegramAccountRepository = telegramAccountRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken token = default
    )
    {
        var user = await _telegramAccountRepository.GetUserByTelegramIdAsync(
            command.TelegramId, 
            token);

        if (user is not null)
        {
            return new RegisterUserResult(user.Id, false);
        }

        var newUser = User.RegisterUserAsATelegramMember(command.DisplayName, 
            command.TelegramId);

        await _userRepository.AddAsync(newUser, token);
        await _unitOfWork.SaveChangesAsync(token);

        return new RegisterUserResult(newUser.Id, true);
    }
}