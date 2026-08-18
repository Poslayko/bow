using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.Users.ConfigureLearning;

public sealed class ConfigureLearningUserHandler
{
    private readonly IUserRepository _user;
    private readonly IUnitOfWork _unit;

    public ConfigureLearningUserHandler(IUserRepository user, IUnitOfWork unit)
    {
        _user = user;
        _unit = unit;
    }

    public async Task HandleAsync(ConfigureLearningUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _user.GetByTelegramIdAsync(command.TelegramId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with TelegramId: {command.TelegramId} was not found");
        }

        user.ConfigureLearning(command.NativeLanguage, command.LearningLanguage, 
            command.LearningLevel);

        await _unit.SaveChangesAsync(cancellationToken);                
    }
}