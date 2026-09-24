using bow.Application.Common.Exceptions;
using bow.Application.Common.Interfaces;
using bow.Domain.Entities;

namespace bow.Application.UserVocabularyProgresses.Add;

public sealed class AddUserVocabularyProgressHandler
{
    private readonly IUserRepository _users;
    private readonly IVocabularyItemRepository _items;
    private readonly IUserVocabularyProgressRepository _userProgresses;
    private readonly IUnitOfWork _unit;

    public AddUserVocabularyProgressHandler(
        IUserRepository users,
        IVocabularyItemRepository items,
        IUserVocabularyProgressRepository userProgresses,
        IUnitOfWork unit
    )
    {
        _users = users;
        _items = items;
        _userProgresses = userProgresses;
        _unit = unit;
    }

    public async Task<AddUserVocabularyProgressResult> HandleAsync(
        AddUserVocabularyProgressCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var possibleUser = await _users.GetByIdAsync(command.UserId, cancellationToken);

        if (possibleUser is not {} user)
        {
            throw new NotFoundException("Wrong data");
        }

        var item = await _items.GetByIdAsync(command.VocabularyItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Vocabulary item with '{command.VocabularyItemId}' wasn't found");
        }

        var userProgress = await _userProgresses.GetByUserAndVocabularyItemAsync(user.Id,
            item.Id, cancellationToken);

        var isCreated = false;

        if (userProgress is null)
        {
            userProgress = new UserVocabularyProgress(user.Id, item.Id, DateTime.UtcNow);
            await _userProgresses.AddAsync(userProgress, cancellationToken);

            isCreated = true;
            await _unit.SaveChangesAsync(cancellationToken);
        }

        return new AddUserVocabularyProgressResult(
            isCreated,
            userProgress.Id,
            userProgress.VocabularyItemId,
            userProgress.UserId,
            userProgress.Stage,
            userProgress.NextReviewAt,
            userProgress.LastReviewedAt
        );
    }
}