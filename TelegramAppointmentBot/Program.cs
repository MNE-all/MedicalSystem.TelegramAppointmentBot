using Microsoft.EntityFrameworkCore.Query.Internal;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Service.Contract.Interfaces;
using TelegramAppointmentBot.Service.Implementation;
using User = TelegramAppointmentBot.Context.Models.User;

class Program
{
    private static IUserService UserService = new UserService();

    private static IProfileService ProfileService = new ProfileService();
    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;

    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {

        _botClient = new TelegramBotClient(Configuration.botToken); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
        _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
        {
            AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
            {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                UpdateType.CallbackQuery
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            ThrowPendingUpdates = true,
        };

        //_botClient.AnswerCallbackQueryAsync

        using var cts = new CancellationTokenSource();

        // UpdateHander - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

        var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("");

        try
        {

            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        // эта переменная будет содержать в себе все связанное с сообщениями
                        var message = update.Message;

                        // From - это от кого пришло сообщение (или любой другой Update)
                        var user = message!.From;


                        switch (message.Text)
                        {
                            case "/start":
                                await UserService.AddUser(new User
                                {
                                    Id = user!.Id,
                                    FirstName = user.FirstName,
                                    CurrentProfile = null,
                                    Statement = ProfileStatement.None
                                }, cancellationToken);

                                var replyKeyboard = new ReplyKeyboardMarkup(
                                    new List<KeyboardButton[]>()
                                    {
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Профили"),
                                    new KeyboardButton("Запись"),
                                },
                                    })
                                {
                                    // автоматическое изменение размера клавиатуры, если не стоит true,
                                    // тогда клавиатура растягивается чуть ли не до луны,
                                    // проверить можете сами
                                    ResizeKeyboard = true,
                                };

                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Для того чтобы отлавливать запись - у вас должен быть как минимум один профиль",
                                    replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup
                                break;

                            case "Профили":
                                var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);

                                await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);
                                await UserService.ClearCurrentProfile(user!.Id, cancellationToken);

                                var buttons = new List<InlineKeyboardButton[]>();
                                foreach (var profile in profiles)
                                {
                                    buttons.Add(new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData(profile.Title, profile.Id.ToString())
                                });
                                }
                                buttons.Add(new[]
                                {
                                InlineKeyboardButton.WithCallbackData("Добавить профиль", "Add")
                            });
                                // Тут создаем нашу клавиатуру
                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Добавьте или измените профиль",
                                    replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup
                                break;

                            default:

                                switch (UserService.CheckStatement(user!.Id, cancellationToken).Result)
                                {
                                    case ProfileStatement.AddTitle:
                                        var profile = ProfileService.AddProfile(user!.Id, message.Text!, cancellationToken);
                                        await UserService.ChangeCurrentProfile(user!.Id, profile.Result.Id, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddOMS, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите номер полиса ОМС");
                                        break;
                                    case ProfileStatement.AddOMS:
                                        Guid profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangeOMS(profileId, message.Text!, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddSurname, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите фамилию пациента");
                                        break;
                                    case ProfileStatement.AddSurname:
                                        profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangeSurname(profileId, message.Text!, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddName, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите имя пациента");
                                        break;
                                    case ProfileStatement.AddName:
                                        profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangeName(profileId, message.Text!, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddPatronomyc, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите отчество пациента");
                                        break;
                                    case ProfileStatement.AddPatronomyc:
                                        profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangePatronomyc(profileId, message.Text!, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddEmail, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите email пациента");
                                        break;
                                    case ProfileStatement.AddEmail:
                                        profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangeEmail(profileId, message.Text!, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.AddBirthdate, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Введите дату рождения пациента");
                                        break;
                                    case ProfileStatement.AddBirthdate:
                                        profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                        await ProfileService.ChangeBirthdate(profileId, DateTime.Parse(message.Text!), cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.Finished, cancellationToken);
                                        await ProfileService.ValidateProfile(profileId, cancellationToken);
                                        profile = ProfileService.GetProfileByIdAsync(profileId, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id,
                                            $"Название профиля: {profile.Result.Title}\n" +
                                            $"ОМС: {profile.Result.OMS}\n" +
                                            $"Фамилия: {profile.Result.Surname}\n" +
                                            $"Имя: {profile.Result.Name}\n" +
                                            $"Отчество: {profile.Result.Patronomyc}\n" +
                                            $"Дата рождения: {profile.Result.Birthdate!.Value.ToString("D")}\n");
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                        break;
                    }
                case UpdateType.CallbackQuery:
                    {
                        // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                        var callbackQuery = update.CallbackQuery;

                        // Аналогично и с Message мы можем получить информацию о чате, о пользователе и т.д.
                        var user = callbackQuery.From;

                        // Выводим на экран нажатие кнопки
                        Console.WriteLine($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");

                        // Вот тут нужно уже быть немножко внимательным и не путаться!
                        // Мы пишем не callbackQuery.Chat , а callbackQuery.Message.Chat , так как
                        // кнопка привязана к сообщению, то мы берем информацию от сообщения.
                        var chat = callbackQuery.Message.Chat;

                        // Добавляем блок switch для проверки кнопок
                        switch (callbackQuery.Data)
                        {
                            // Data - это придуманный нами id кнопки, мы его указывали в параметре
                            // callbackData при создании кнопок. У меня это button1, button2 и button3

                            case "Add":
                                {
                                    // В этом типе клавиатуры обязательно нужно использовать следующий метод
                                    //await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                    // Для того, чтобы отправить телеграмму запрос, что мы нажали на кнопку

                                    UserService.ChangeStatement(user.Id, ProfileStatement.AddTitle, cancellationToken);

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"Введите название профиля");
                                    return;
                                }

                            case "button2":
                                {
                                    // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Тут может быть ваш текст!");

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"Вы нажали на {callbackQuery.Data}");
                                    return;
                                }

                            case "button3":
                                {
                                    // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "А это полноэкранный текст!", showAlert: true);

                                    await botClient.SendTextMessageAsync(
                                        chat.Id,
                                        $"Вы нажали на {callbackQuery.Data}");
                                    return;
                                }
                        }

                        return;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}