using bow.Application.Common.Interfaces;
using bow.Application.Telegram.Onboarding;
using bow.Application.Telegram.Registration;
using bow.Application.Telegram.Study.StartNext;
using bow.Application.Telegram.Study.SubmitAnswer;
using bow.Domain.Entities;
using bow.Domain.Enums;

namespace bow.Application.Telegram;

public sealed class HandleTelegramInputHandler
{
    private readonly ITelegramAccountRepository _telegramAccountRepository;
    private readonly RegisterTelegramUserHandler _registerTelegramUserHandler;
    private readonly ChooseNativeLanguageHandler _chooseNativeLanguageHandler;
    private readonly SelectNativeLanguageHandler _selectNativeLanguageHandler;
    private readonly ChooseLearningLanguageHandler _chooseLearningLanguageHandler;
    private readonly SelectLearningLanguageHandler _selectLearningLanguageHandler;
    private readonly ChooseLearningLevelHandler _chooseLearningLevelHandler;
    private readonly SelectLearningLevelHandler _selectLearningLevelHandler;
    private readonly ShowMenuHandler _showMenuHandler;
    private readonly ITelegramMessageSender _sender;
    private readonly StartNextStudyItemHandler _startNextStudyItemHandler;
    private readonly SubmitStudyAnswerHandler _submitStudyAnswerHandler;
    private readonly ShowLearningMenuHandler _showLearningMenuHandler;

    public HandleTelegramInputHandler(ITelegramAccountRepository telegramAccountRepository,
        RegisterTelegramUserHandler registerTelegramUserHandler,
        ChooseNativeLanguageHandler chooseNativeLanguageHandler,
        SelectNativeLanguageHandler selectNativeLanguageHandler,
        ChooseLearningLanguageHandler chooseLearningLanguageHandler,
        SelectLearningLanguageHandler selectLearningLanguageHandler,
        ChooseLearningLevelHandler chooseLearningLevelHandler,
        SelectLearningLevelHandler selectLearningLevelHandler,
        ShowMenuHandler showMenuHandler,
        ITelegramMessageSender sender,
        StartNextStudyItemHandler startNextStudyItemHandler,
        SubmitStudyAnswerHandler submitStudyAnswerHandler,
        ShowLearningMenuHandler showLearningMenuHandler)
    {
        _telegramAccountRepository = telegramAccountRepository;
        _registerTelegramUserHandler = registerTelegramUserHandler;
        _chooseNativeLanguageHandler = chooseNativeLanguageHandler;
        _selectNativeLanguageHandler = selectNativeLanguageHandler;
        _chooseLearningLanguageHandler = chooseLearningLanguageHandler;
        _selectLearningLanguageHandler = selectLearningLanguageHandler;
        _chooseLearningLevelHandler = chooseLearningLevelHandler;
        _selectLearningLevelHandler = selectLearningLevelHandler;
        _showMenuHandler = showMenuHandler;
        _sender = sender;
        _startNextStudyItemHandler = startNextStudyItemHandler;
        _submitStudyAnswerHandler = submitStudyAnswerHandler;
        _showLearningMenuHandler = showLearningMenuHandler;
    }

    public async Task HandleAsync(
        TelegramInput input,
        CancellationToken cancellationToken
    )
    {
        var user = await _telegramAccountRepository.GetUserByTelegramIdAsync(
            input.TelegramUserId, 
            cancellationToken);
                
        TelegramAccount telegramAccount;

        if (user is null)
        {
            user = await _registerTelegramUserHandler.HandleAsync(input, 
                cancellationToken);

            telegramAccount = user.TelegramAccount
                ?? throw new InvalidOperationException(
                    "User didn't have Telegram account, but was regostered as telegram user"
                );
            
            await _chooseNativeLanguageHandler.HandleAsync(user.TelegramAccount.TelegramId, 
            telegramAccount, cancellationToken);

            return;
        }  

       telegramAccount = user.TelegramAccount
                ?? throw new InvalidOperationException(
                    "User didn't have Telegram account, but was regostered as telegram user"
                ); 

        if (input.CallbackData is not null)
        {
            var parts = input.CallbackData.Split(':', 2);

            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return;
            }

            var action = parts[0];
            var value = parts[1];

            switch ((action, telegramAccount.State))
            {
                case ("native-language", TelegramUserState.ChoosingNativeLanguage):
                    if (!Enum.TryParse<LanguageCode>(value, ignoreCase: true, 
                        out var languageCode)
                        || !Enum.IsDefined(languageCode))
                    {
                        return;
                    }

                    await _selectNativeLanguageHandler.HandleAsync(user, languageCode, 
                        cancellationToken);
                    await _chooseLearningLanguageHandler.HandleAsync(telegramAccount.TelegramId,
                        telegramAccount, cancellationToken);
                    break;
                
                case ("learning-language", TelegramUserState.ChoosingLearningLanguage):
                    if (user.NativeLanguage is not {} nativeLanguage)
                    {
                        return;
                    }

                    if (!Enum.TryParse<LanguageCode>(value, ignoreCase: true, 
                        out var learningLanguage)
                        || !Enum.IsDefined(learningLanguage))
                    {
                        return;
                    }

                    if (nativeLanguage == learningLanguage)
                    {
                        await _sender.SendErrorLearningAndNativeLanguageCanNotBeTheSame(
                            telegramId: telegramAccount.TelegramId, 
                            cancellationToken
                        );

                        await _chooseLearningLanguageHandler.HandleAsync(telegramAccount.TelegramId,
                            telegramAccount, cancellationToken);

                        break;
                    }
                    
                    await _selectLearningLanguageHandler.HandleAsync(user, value, 
                        cancellationToken);
                    await _chooseLearningLevelHandler.HandleAsync(telegramAccount.TelegramId,
                        telegramAccount, cancellationToken);
                    break;

                case ("learning-level", TelegramUserState.ChoosingLearningLevel):
                    if (!Enum.TryParse<CefrLevel>(value, ignoreCase: true, out var cefrLevel)
                        || !Enum.IsDefined(cefrLevel))
                    {
                        return;
                    }

                    await _selectLearningLevelHandler.HandleAsync(user, cefrLevel,
                        cancellationToken);
                    await _showMenuHandler.HandleAsync(telegramAccount.TelegramId,
                        telegramAccount, cancellationToken);
                    break;

            }

            return;
        }

        if (input.Text is not {} text)
            {
                await _sender.SendTextMsg(
                    telegramAccount.TelegramId,
                    "No text",
                    cancellationToken);

                return;
            }

        switch (telegramAccount.State, text)
        {
            case (TelegramUserState.Registred, _):
                await _chooseNativeLanguageHandler.HandleAsync(
                    telegramAccount.TelegramId, 
                    telegramAccount, 
                    cancellationToken
                );

                break;

            case (TelegramUserState.ChoosingNativeLanguage, _): 
                await _chooseNativeLanguageHandler.HandleAsync(
                    telegramAccount.TelegramId,
                    telegramAccount, 
                    cancellationToken
                );

                break;

            case (TelegramUserState.ChoosingInMenu, "📊 Show results"):
                await _sender.SendTextMsg(
                    telegramAccount.TelegramId,
                    "This option is not implemented",
                    cancellationToken
                );

                break;

            case (TelegramUserState.ChoosingInMenu, "⚙️ Settings"):
                await _sender.SendTextMsg(
                    telegramAccount.TelegramId,
                    "This options is not implemented",
                    cancellationToken
                );

                break;

            case (TelegramUserState.ChoosingInMenu, "📚 Start learning words"):
                await _showLearningMenuHandler.HandleAsync(
                    telegramAccount.TelegramId,
                    telegramAccount,
                    cancellationToken
                );

                await _startNextStudyItemHandler.HandleAsync(
                    user,
                    telegramAccount,
                    telegramAccount.TelegramId,
                    cancellationToken
                );

                break; 
            
            case (TelegramUserState.SubmitAnswer, "🚫 Stop learning new items"):
                await _showMenuHandler.HandleAsync(
                    telegramAccount.TelegramId,
                    telegramAccount,
                    cancellationToken
                );

                break;

            case (TelegramUserState.SubmitAnswer, _):
                var result_in_checking_answer = await _submitStudyAnswerHandler.HandleAsync(
                    user,
                    text,
                    telegramAccount.TelegramId,
                    cancellationToken
                );

                if (result_in_checking_answer == SubmitAnswerResult.NotProcessed)
                {
                    break;
                }

                await _startNextStudyItemHandler.HandleAsync(
                    user,
                    telegramAccount,
                    telegramAccount.TelegramId,
                    cancellationToken
                );

                break;
        }
    }
}