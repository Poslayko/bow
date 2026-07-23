using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Users.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userRepository.GetByTelegramIdAsync(
            command.TelegramId, 
            cancellationToken);

        if (user is not null)
        {
            return new RegisterUserResult(user.Id, false);
        }

        var newUser = new User(command.TelegramId, command.DisplayName);

        await _userRepository.AddAsync(newUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(newUser.Id, true);
    }
}