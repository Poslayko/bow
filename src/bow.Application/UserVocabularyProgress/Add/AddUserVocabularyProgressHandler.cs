using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.UserVocabularyProgresses.Add;

public sealed class AddUserVocabularyProgressHandler
{
    private readonly IUserRepository _user;
    private readonly IVocabularyItemRepository _item;
    private readonly IUserVocabularyProgressRepository _userProgress;
    private readonly IUnitOfWork _unit;

    public AddUserVocabularyProgressHandler(
        IUserRepository user,
        IVocabularyItemRepository item,
        IUserVocabularyProgressRepository userProgress,
        IUnitOfWork unit
    )
    {
        _user = user;
        _item = item;
        _userProgress = userProgress;
        _unit = unit;
    }

    public async Task<AddUserVocabularyProgressResult> HandleAsync(
        AddUserVocabularyProgressCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _user.GetByTelegramIdAsync(
            command.TelegramId,
            cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException($"User with '{command.TelegramId}' wasn't found");
        }

        var item = await _item.GetByIdAsync(command.VocabularyItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Vocabulary item with '{command.VocabularyItemId}' wasn't found");
        }

        var userProgress = await _userProgress.GetByUserAndVocabularyItemAsync(user.Id,
            item.Id, cancellationToken);

        var isCreated = false;

        if (userProgress is null)
        {
            userProgress = new UserVocabularyProgress(user.Id, item.Id, DateTime.UtcNow);
            await _userProgress.AddAsync(userProgress, cancellationToken);

            isCreated = true;
            await _unit.SaveChangesAsync(cancellationToken);
        }

        return new AddUserVocabularyProgressResult(
            isCreated,
            userProgress.Id,
            userProgress.Stage,
            userProgress.UserId,
            userProgress.NextReviewAt
        );
    }
}