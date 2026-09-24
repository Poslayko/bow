using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;

namespace bow.Application.Users.ConfigureLearning;

public sealed class ConfigureLearningUserHandler
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unit;

    public ConfigureLearningUserHandler(IUserRepository users, IUnitOfWork unit)
    {
        _users = users;
        _unit = unit;
    }

    public async Task HandleAsync(ConfigureLearningUserCommand command,
        CancellationToken cancellationToken)
    {
        var possibleUser = await _users.GetByIdAsync(command.UserId, cancellationToken);

        if (possibleUser is not {} user)
        {
            throw new NotFoundException("Wrong data");
        }

        user.ConfigureLearning(command.NativeLanguage, command.LearningLanguage, 
            command.LearningLevel);

        await _unit.SaveChangesAsync(cancellationToken);                
    }
}