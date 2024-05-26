using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;
using TelegramAppointmentBot.Service.Implementation;
using User = TelegramAppointmentBot.Context.Models.User;

class Program
{
    private static IUserService UserService = new UserService();

    private static IProfileService ProfileService = new ProfileService();

    private static IGorzdravService GorzdravService = new GorzdravService();

    private static IAppointmentHunterService AppointmentHunterService = new AppointmentHunterService();

    private static IVisitService VisitService = new VisitService();

    private static ISpecialityService SpecialityService = new SpecialityService();



    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;

    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

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



        // Запуск регулярной попытки создать запись
        TimerCallback tm = new TimerCallback(TryToWrite);
        Timer timer = new Timer(tm, null, 0, 1000 * 60 * 5);
        //


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
        try
        {
            _ = Task.Run(async () =>
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
                                    Guid? hunter;
                                    await ClearUserMemory(user, cancellationToken);

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
                                    await ClearUserMemory(user, cancellationToken);
                                    var profiles = await ProfileService.GetUserProfilesAsync(user!.Id, cancellationToken);

                                    var buttons = new List<InlineKeyboardButton[]>();
                                    foreach (var profile in profiles)
                                    {
                                        buttons.Add(new[]
                                        {
                                        InlineKeyboardButton.WithCallbackData(profile.Title, (int)InlineMode.ProfileInfo + ":" + profile.Id.ToString())
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

                                case "Запись":
                                    await ClearUserMemory(user, cancellationToken);


                                    buttons = new List<InlineKeyboardButton[]>
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Просмотр визитов", "ShowVisits")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Просмотр записей", "ShowAppointments")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Добавить запись", "AddAppointment")
                                        }
                                    };

                                    inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                    await botClient.SendTextMessageAsync(
                                        user.Id,
                                        "Выберите что вам нужно:",
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
                                            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            await ProfileService.ChangeOMS(profileId.Value, message.Text!, cancellationToken);
                                            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddSurname, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите фамилию пациента");
                                            break;
                                        case ProfileStatement.AddSurname:
                                            profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            await ProfileService.ChangeSurname(profileId.Value, message.Text!, cancellationToken);
                                            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddName, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите имя пациента");
                                            break;
                                        case ProfileStatement.AddName:
                                            profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            await ProfileService.ChangeName(profileId.Value, message.Text!, cancellationToken);
                                            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddPatronomyc, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите отчество пациента");
                                            break;
                                        case ProfileStatement.AddPatronomyc:
                                            profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            await ProfileService.ChangePatronomyc(profileId.Value, message.Text!, cancellationToken);
                                            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddEmail, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите email пациента");
                                            break;
                                        case ProfileStatement.AddEmail:
                                            profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            await ProfileService.ChangeEmail(profileId.Value, message.Text!, cancellationToken);
                                            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddBirthdate, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите дату рождения пациента");
                                            break;
                                        case ProfileStatement.AddBirthdate:
                                            profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
                                            if (DateTime.TryParse(message.Text!, out var bithdate))
                                            {
                                                await ProfileService.ChangeBirthdate(profileId.Value, bithdate, cancellationToken);
                                                await UserService.ChangeStatement(user!.Id, ProfileStatement.Finished, cancellationToken);
                                                await ProfileService.ValidateProfile(profileId.Value, cancellationToken);
                                                profile = ProfileService.GetProfileByIdAsync(profileId.Value, cancellationToken);
                                                await botClient.SendTextMessageAsync(user.Id,
                                                    $"Название профиля: {profile.Result.Title}\n" +
                                                    $"ОМС: {profile.Result.OMS}\n" +
                                                    $"Фамилия: {profile.Result.Surname}\n" +
                                                    $"Имя: {profile.Result.Name}\n" +
                                                    $"Отчество: {profile.Result.Patronomyc}\n" +
                                                    $"Дата рождения: {profile.Result.Birthdate!.Value.ToString("d MMMM yyyy")}\n");
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(user.Id, "Данные в неверном виде\n" +
                                                    "Попробуйте ввести дату в формате дд.мм.гггг");
                                            }

                                            break;
                                        case ProfileStatement.AddTime:
                                            hunter = await UserService.GetCurrentHunter(user.Id, cancellationToken);

                                            replyKeyboard = new ReplyKeyboardMarkup(
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

                                            if (message.Text! == "Любой")
                                            {
                                                await botClient.SendTextMessageAsync(user.Id, "Запись отслеживается", replyMarkup: replyKeyboard);
                                                await AppointmentHunterService.ChangeStatement(hunter.Value, HunterStatement.InProgress, cancellationToken);
                                                await ClearUserMemory(user, cancellationToken);
                                                return;
                                            }

                                            var userInput = message.Text!.Split('-');

                                            if (userInput.Length == 1)
                                            {
                                                if (DateTime.TryParse(message.Text!, out var date))
                                                {
                                                    await AppointmentHunterService.ChangeTime(hunter.Value, date, cancellationToken);
                                                    await botClient.SendTextMessageAsync(user.Id, "Запись отслеживается", replyMarkup: replyKeyboard);
                                                    await AppointmentHunterService.ChangeStatement(hunter.Value, HunterStatement.InProgress, cancellationToken);
                                                    await ClearUserMemory(user, cancellationToken);
                                                    return;
                                                }
                                            }
                                            else if (userInput.Length == 2)
                                            {
                                                if (DateTime.TryParse(userInput[0], out var dateFrom) && DateTime.TryParse(userInput[1], out var dateTo))
                                                {
                                                    await AppointmentHunterService.ChangeTime(hunter.Value, dateFrom, dateTo, cancellationToken);
                                                    await botClient.SendTextMessageAsync(user.Id, "Запись отслеживается", replyMarkup: replyKeyboard);
                                                    await AppointmentHunterService.ChangeStatement(hunter.Value, HunterStatement.InProgress, cancellationToken);
                                                    await ClearUserMemory(user, cancellationToken);
                                                    return;
                                                }
                                            }
                                            await botClient.SendTextMessageAsync(user.Id, "Данные в неверном формате");
                                            return;
                                        case ProfileStatement.ChangeTitle:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangeTitle(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Название профиля измененно!");
                                            break;
                                        case ProfileStatement.ChangeOMS:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangeOMS(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Номер полиса ОМС изменён!");
                                            break;
                                        case ProfileStatement.ChangeSurname:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangeSurname(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Фамилия изменена!");
                                            break;
                                        case ProfileStatement.ChangeName:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangeName(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Имя измененно!");
                                            break;
                                        case ProfileStatement.ChangePatronomyc:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangePatronomyc(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Отчество измененно!");
                                            break;
                                        case ProfileStatement.ChangeEmail:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                            await ProfileService.ChangeEmail(profileId.Value, message.Text!, cancellationToken);
                                            await ClearUserMemory(user, cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Адрес электронной почты изменён!");
                                            break;
                                        case ProfileStatement.ChangeBirthdate:
                                            profileId = await UserService.GetCurrentProfile(user.Id, cancellationToken);

                                            if (DateTime.TryParse(message.Text!, out bithdate))
                                            {
                                                await ProfileService.ChangeBirthdate(profileId.Value, DateTime.Parse(message.Text!), cancellationToken);
                                                await ClearUserMemory(user, cancellationToken);
                                                await botClient.SendTextMessageAsync(user.Id, "Дата рождения изменена!");
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(user.Id, "Неверный тип данных");
                                            }
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

                            if (callbackQuery.Data == "Add")
                            {
                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                await UserService.ChangeStatement(user.Id, ProfileStatement.AddTitle, cancellationToken);

                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Введите название профиля");
                                return;
                            }
                            else if (callbackQuery.Data == "AddAppointment")
                            {
                                var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);
                                var buttons = new List<InlineKeyboardButton[]>();
                                foreach (var profile in profiles)
                                {
                                    buttons.Add(new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData(profile.Title, (int)InlineMode.AppointmentProfileId + ":" + profile.Id.ToString())
                                });
                                }
                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);


                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Выберите кого хотите записать:",
                                    replyMarkup: inlineKeyboard);
                                return;
                            }
                            else if (callbackQuery.Data == "ShowVisits")
                            {
                                var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);
                                var buttons = new List<InlineKeyboardButton[]>();
                                foreach (var profile in profiles)
                                {
                                    buttons.Add(new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData(profile.Title, (int)InlineMode.VisitsShow + ":" + profile.Id.ToString())
                                });
                                }
                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Выберите чьи визиты желаете просмотреть:",
                                    replyMarkup: inlineKeyboard);
                                return;
                            }
                            else if (callbackQuery.Data == "ShowAppointments")
                            {
                                var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);
                                var buttons = new List<InlineKeyboardButton[]>();
                                foreach (var profile in profiles)
                                {
                                    buttons.Add(new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData(profile.Title, (int)InlineMode.HuntersShow + ":" + profile.Id.ToString())
                                });
                                }
                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Выберите чьи записи желаете просмотреть:",
                                    replyMarkup: inlineKeyboard);
                                return;
                            }

                            if (callbackQuery.Data!.Split(":").Length > 1)
                            {
                                var list = callbackQuery.Data!.Split(":");
                                InlineMode type = (InlineMode)int.Parse(list[0]);
                                // Добавляем блок switch для проверки кнопок
                                switch (type)
                                {
                                    case InlineMode.AppointmentProfileId:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            var getLPUs = await GorzdravService.GetLPUs(Guid.Parse(list[1]), cancellationToken);

                                            if (getLPUs.success)
                                            {
                                                await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);

                                                var buttons = new List<InlineKeyboardButton[]>();
                                                foreach (var lpu in getLPUs.result)
                                                {
                                                    buttons.Add(new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData(lpu.lpuFullName, (int)InlineMode.AppointmentLPUs + ":" + lpu.id.ToString())
                                                    });
                                                }
                                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    $"Выберите медицинское учереждение:",
                                                    replyMarkup: inlineKeyboard);
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, getLPUs.message!, getLPUs.errorCode, cancellationToken);
                                            }

                                            return;
                                        }

                                    case InlineMode.AppointmentLPUs:
                                        {
                                            var specs = await GorzdravService.GetSpecialties(int.Parse(list[1]), cancellationToken);
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            if (specs.success)
                                            {
                                                var buttons = new List<InlineKeyboardButton[]>();
                                                foreach (var specialty in specs.result)
                                                {
                                                    var specialityId = await SpecialityService.AddOrFind(specialty.id, specialty.name, int.Parse(list[1]), cancellationToken);
                                                    buttons.Add(new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData(specialty.name, (int)InlineMode.AppointmentSpecialities + ":" + specialityId)
                                                    });
                                                }
                                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                                await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                $"Выберите направление:",
                                                replyMarkup: inlineKeyboard);
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, specs.message!, specs.errorCode, cancellationToken);
                                            }

                                            return;
                                        }
                                
                                    case InlineMode.AppointmentSpecialities:
                                        {
                                            var speciality = await SpecialityService.GetBySystemId(Guid.Parse(list[1]), cancellationToken);

                                            var getDoctor = await GorzdravService.GetDoctors(speciality.lpuId, speciality.id, cancellationToken);


                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            if (getDoctor.success)
                                            {
                                                var buttons = new List<InlineKeyboardButton[]>();
                                                foreach (var doctor in getDoctor.result)
                                                {
                                                    buttons.Add(new[]
                                                    {
                                                    InlineKeyboardButton.WithCallbackData(doctor.name, (int)InlineMode.AppointmentDoctor + ":" + doctor.id + ":" + speciality.SystemId)
                                                });
                                                }
                                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);


                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    $"Выберите врача:",
                                                    replyMarkup: inlineKeyboard);
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, getDoctor.message!, getDoctor.errorCode, cancellationToken);

                                            }


                                            return;
                                        }
                                    case InlineMode.AppointmentDoctor:
                                        {
                                            var speciality = await SpecialityService.GetBySystemId(Guid.Parse(list[2]), cancellationToken);
                                                                                        
                                            var timetable = await GorzdravService.GetTimetable(speciality.lpuId, int.Parse(list[1]), cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            if (timetable.success)
                                            {
                                                var hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);
                                                if (hunter != null && await AppointmentHunterService.GetStatement(hunter.Value, cancellationToken) == HunterStatement.None)
                                                {
                                                    await AppointmentHunterService.Delete(hunter.Value, cancellationToken);
                                                }

                                                var profileId = await UserService.GetCurrentProfile(user.Id!, cancellationToken);

                                                var hunterId = await AppointmentHunterService.Create(profileId.Value, speciality.lpuId,
                                                speciality.name, int.Parse(list[1]), cancellationToken);
                                                await UserService.ChangeCurrentHunter(user.Id!, hunterId, cancellationToken);


                                                var buttons = new List<InlineKeyboardButton[]>();
                                                if (timetable.result != null)
                                                {

                                                    var days = new List<List<TimetableDayResult>>
                                                    {
                                                        timetable.result.Where(x => x.visitEnd.DayOfWeek == System.DayOfWeek.Monday).ToList(),
                                                        timetable.result.Where(x => x.visitEnd.DayOfWeek == System.DayOfWeek.Tuesday).ToList(),
                                                        timetable.result.Where(x => x.visitEnd.DayOfWeek == System.DayOfWeek.Wednesday).ToList(),
                                                        timetable.result.Where(x => x.visitEnd.DayOfWeek == System.DayOfWeek.Thursday).ToList(),
                                                        timetable.result.Where(x => x.visitEnd.DayOfWeek == System.DayOfWeek.Friday).ToList()
                                                    };
                                                    var daysNames = new List<string> { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница" };

                                                    foreach (var day in days)
                                                    {
                                                        if (day.Count != 0)
                                                        {
                                                            buttons.Add(new[]
                                                            {
                                                                InlineKeyboardButton.WithCallbackData($"{daysNames[days.IndexOf(day)]}: " +
                                                                new DateTime(day.Min(x => x.visitStart.TimeOfDay).Ticks).ToString("t") +
                                                                " - " +
                                                                new DateTime(day.Max(x => x.visitEnd.TimeOfDay).Ticks).ToString("t"),
                                                                $"{(int)InlineMode.AppointmentDay}:{(int)day.First().visitStart.DayOfWeek}")
                                                            });
                                                        }
                                                    }
                                                    buttons.Add(new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData($"Любой",
                                                        $"{(int)InlineMode.AppointmentDay}:Любой")
                                                    });

                                                    var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                                    await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                        "Расписание врача: ",
                                                        replyMarkup: inlineKeyboard);
                                                }
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, timetable.message!, timetable.errorCode, cancellationToken);
                                            }
                                            return;
                                        }
                                    case InlineMode.AppointmentDay:
                                        {
                                            var hunterId = await UserService.GetCurrentHunter(user.Id!, cancellationToken);
                                            var hunter = await AppointmentHunterService.GetHunterById(hunterId.Value, cancellationToken);

                                            var timetable = await GorzdravService.GetTimetable(hunter.LpuId, hunter.DoctorId!.Value, cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            System.DayOfWeek? dayOfWeek = null;

                                            if (int.TryParse(list[1], out var dayOfWeekInt))
                                            {
                                                dayOfWeek = (System.DayOfWeek)dayOfWeekInt;
                                                await AppointmentHunterService.ChangeDayOfWeek(hunterId.Value, dayOfWeek.Value, cancellationToken);
                                            }

                                            if (timetable.success)
                                            {
                                                string answer = "Введите интересующее время посещения или желаемый диапозон времени через дефис\n" +
                                                "пример: 12:00 - 13:00\n" +
                                                "(в рамках: ";
                                                if (timetable.result != null)
                                                {

                                                    List<TimetableDayResult> times = new();

                                                    if (dayOfWeek != null)
                                                    {
                                                        times = timetable.result.Where(x => x.visitEnd.DayOfWeek == dayOfWeek).ToList();
                                                    }
                                                    else
                                                    {
                                                        times = timetable.result.ToList();
                                                    }

                                                    await AppointmentHunterService.ChangeDayOfWeek(hunterId.Value, dayOfWeek, cancellationToken);

                                                    List<TimeFrame> timeFrameList = new();
                                                    foreach (var t in times)
                                                    {
                                                        timeFrameList.Add(new TimeFrame
                                                        {
                                                            TimeStart = t.visitStart.TimeOfDay,
                                                            TimeEnd = t.visitEnd.TimeOfDay
                                                        });
                                                    }

                                                    var distinctTimes = timeFrameList.Select(x => new { x.TimeStart, x.TimeEnd }).Distinct().ToList();

                                                    foreach (var frame in distinctTimes)
                                                    {
                                                        answer += new DateTime(frame.TimeStart.Ticks).ToString("t") +
                                                                " - " +
                                                                new DateTime(frame.TimeEnd.Ticks).ToString("t");

                                                        if (distinctTimes.IndexOf(frame) != distinctTimes.Count - 1)
                                                        {
                                                            answer += ", ";
                                                        }
                                                        else
                                                        {
                                                            answer += "): ";
                                                        }
                                                    }
                                                    
                                                    var replyKeyboard = new ReplyKeyboardMarkup(
                                                        new List<KeyboardButton[]>()
                                                        {
                                                            new KeyboardButton[]
                                                            {
                                                                new KeyboardButton("Любой")
                                                            },
                                                        })
                                                    {
                                                        // автоматическое изменение размера клавиатуры, если не стоит true,
                                                        // тогда клавиатура растягивается чуть ли не до луны,
                                                        // проверить можете сами
                                                        ResizeKeyboard = true,
                                                    };

                                                    await UserService.ChangeStatement(user.Id, ProfileStatement.AddTime, cancellationToken);
                                                    await botClient.SendTextMessageAsync(chat.Id, answer, replyMarkup: replyKeyboard);
                                                }
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, timetable.message!, timetable.errorCode, cancellationToken);
                                            }
                                            return;
                                        }
                                    case InlineMode.VisitsShow:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            var profileId = Guid.Parse(list[1]);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");

                                            var getLpus = await GorzdravService.GetLPUs(profileId, cancellationToken);

                                            //TODO Можно сделать разбивку по мед. учреждениям + поиск по всем

                                            var buttons = new List<InlineKeyboardButton[]>();


                                            List<VisitResult> results = new List<VisitResult>();
                                            if (getLpus.success)
                                            {
                                                
                                                foreach (var lpu in getLpus.result)
                                                {
                                                    buttons.Add(new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData($"{lpu.lpuFullName}",
                                                        $"{(int)InlineMode.LpuVisitsShow}:{profileId}:{lpu.id}")
                                                    });
                                                }
                                                buttons.Add(new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData($"Все",
                                                    $"{(int)InlineMode.LpuVisitsShow}:{profileId}:Все")
                                                });

                                                var inlineKeyboard = new InlineKeyboardMarkup(buttons);


                                                await botClient.SendTextMessageAsync(chat.Id, "Выберите в каком медицинском учреждении желаете просмотреть предстоящие визиты: ",
                                                    replyMarkup: inlineKeyboard);
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, getLpus.message!, getLpus.errorCode, cancellationToken);
                                            }

                                            return;
                                        }
                                    case InlineMode.LpuVisitsShow:
                                        {
                                            await ClearUserMemory(user, cancellationToken);

                                            var profileId = Guid.Parse(list[1]);
                                            var profile = await ProfileService.GetProfileByIdAsync(profileId, cancellationToken);

                                            List<VisitResult> results = new List<VisitResult>();

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");


                                            if (list[2] == "Все")
                                            {
                                                var getLpus = await GorzdravService.GetLPUs(profileId, cancellationToken);

                                                
                                                if (getLpus.success)
                                                {

                                                    foreach (var lpu in getLpus.result)
                                                    {
                                                        var patientGet = await GorzdravService.GetPatient(profileId, lpu.id, cancellationToken);

                                                        if (!patientGet.success)
                                                        {
                                                            await botClient.SendTextMessageAsync(chat.Id, $"{lpu.lpuFullName}\n" +
                                                                $"{patientGet.message}");
                                                        }
                                                        else
                                                        {
                                                            var response = await GorzdravService.GetVisits(patientGet.result, lpu.id, cancellationToken);
                                                            if (response.success && response.result != null)
                                                            {
                                                                results.AddRange(response.result);
                                                            }
                                                            else
                                                            {
                                                                GorzdravError(chat.Id, response!.message!, response.errorCode, cancellationToken);
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    GorzdravError(chat.Id, getLpus.message!, getLpus.errorCode, cancellationToken);
                                                    return;
                                                }
                                            }
                                            else if (int.TryParse(list[2], out var lpuId))
                                            {
                                                var patientGet = await GorzdravService.GetPatient(profileId, lpuId, cancellationToken);

                                                if (patientGet.success)
                                                {
                                                    var response = await GorzdravService.GetVisits(patientGet.result, lpuId, cancellationToken);
                                                    if (response.success && response.result != null)
                                                    {
                                                        results.AddRange(response.result);
                                                    }
                                                    else
                                                    {
                                                        GorzdravError(chat.Id, response!.message!, response.errorCode, cancellationToken);
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    GorzdravError(chat.Id, patientGet.message!, patientGet.errorCode, cancellationToken);
                                                    return;
                                                }
                                            }

                                            foreach (var result in results)
                                            {
                                                var visitId = await VisitService.AddVisit(new Visit
                                                {
                                                    appointmentId = result.appointmentId,
                                                    lpuId = result.lpuId,
                                                    patientId = result.patientId
                                                }, cancellationToken);
                                                var button = InlineKeyboardButton.WithCallbackData("Отменить", (int)InlineMode.DeleteVisit + ":" + visitId);
                                                var inlineKeyboard = new InlineKeyboardMarkup(button);

                                                await botClient.SendTextMessageAsync(chat.Id,
                                                    $"Пациент: {profile.Surname} {profile.Name}\n" +
                                                    $"{result.lpuShortName}\n" +
                                                    $"{result.specialityRendingConsultation.name}\n" +
                                                    $"{result.visitStart.ToString("f")}\n" +
                                                    $"", replyMarkup: inlineKeyboard);
                                            }

                                            if (results.Count == 0)
                                            {
                                                await botClient.SendTextMessageAsync(chat.Id, "Предстоящие визиты отсутсвуют");
                                            }
                                            return;
                                        }
                                    case InlineMode.HuntersShow:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            var profileId = Guid.Parse(list[1]);

                                            var hunters = await AppointmentHunterService.GetHuntersInProgressByProfileId(profileId, cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);


                                            foreach (var hunter in hunters)
                                            {
                                                var button = InlineKeyboardButton.WithCallbackData("Отменить", (int)InlineMode.DeleteHunter + ":" + hunter.Id);
                                                var inlineKeyboard = new InlineKeyboardMarkup(button);

                                                string dayOfWeek = "Любой";
                                                if (hunter.DesiredDay != null)
                                                {
                                                    dayOfWeek = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(hunter.DesiredDay!.Value);
                                                }

                                                string desiredTime = "Любое";

                                                if (hunter.DesiredTime != null)
                                                {
                                                    desiredTime = hunter.DesiredTime.Value.ToString("t");
                                                }
                                                else if (hunter.DesiredTimeFrom != null && hunter.DesiredTimeTo != null)
                                                {
                                                    desiredTime = $"{hunter.DesiredTimeFrom.Value.ToString("t")} - {hunter.DesiredTimeTo.Value.ToString("t")}";
                                                }


                                                await botClient.SendTextMessageAsync(chat.Id,
                                                    $"К кому: {hunter.SpecialityName}\n" +
                                                    $"Желаемое время: {desiredTime}\n" +
                                                    $"Желаемый день недели: {char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1)}", replyMarkup: inlineKeyboard);
                                            }
                                            if (hunters.Count == 0)
                                            {
                                                await botClient.SendTextMessageAsync(chat.Id, "Запись к врачу не отслеживается");
                                            }
                                            return;
                                        }
                                    case InlineMode.DeleteHunter:
                                        await ClearUserMemory(user, cancellationToken);
                                        await AppointmentHunterService.Delete(Guid.Parse(list[1]), cancellationToken);
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                        await botClient.SendTextMessageAsync(chat.Id, "Запись удалена!");
                                        return;

                                    case InlineMode.DeleteVisit:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            var visitInfo = await VisitService.GetVisit(Guid.Parse(list[1]), cancellationToken);
                                            // TODO Можно сделать проверку ответа запроса
                                            var response = await GorzdravService.DeleteAppointment(new TelegramAppointmentBot.Context.Models.Request.CancelTheAppointment
                                            {
                                                appointmentId = visitInfo.appointmentId,
                                                lpuId = visitInfo.lpuId,
                                                patientId = visitInfo.patientId
                                            }, cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            if (response.result)
                                            {
                                                await botClient.SendTextMessageAsync(chat.Id, "Запись отменена!");
                                            }
                                            else
                                            {
                                                GorzdravError(chat.Id, response.message!, response.errorCode, cancellationToken);
                                            }

                                            return;
                                        }

                                    case InlineMode.ProfileInfo:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            var profile = await ProfileService.GetProfileByIdAsync(Guid.Parse(list[1]), cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            var buttons = new List<InlineKeyboardButton[]>
                                            {
                                                new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Изменить", (int)InlineMode.ChangeProfile + ":" + list[1])
                                            },
                                                new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Удалить", (int)InlineMode.DeleteProfile + ":" + list[1])
                                            }
                                            };
                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                            string birthdate = "";
                                            if (profile.Birthdate.HasValue)
                                            {
                                                birthdate = profile.Birthdate.Value.ToString("d MMMM yyyy");
                                            }

                                            await botClient.SendTextMessageAsync(user.Id,
                                                    $"Название профиля: {profile.Title}\n" +
                                                    $"ОМС: {profile.OMS}\n" +
                                                    $"Фамилия: {profile.Surname}\n" +
                                                    $"Имя: {profile.Name}\n" +
                                                    $"Отчество: {profile.Patronomyc}\n" +
                                                    $"Дата рождения: {birthdate}\n",
                                                    replyMarkup: inlineKeyboard);


                                            return;
                                        }
                                    case InlineMode.DeleteProfile:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            await ProfileService.Delete(Guid.Parse(list[1]), cancellationToken);

                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            await botClient.SendTextMessageAsync(user.Id, "Профиль удалён!");
                                            return;
                                        }
                                    case InlineMode.ChangeProfile:
                                        {
                                            await ClearUserMemory(user, cancellationToken);
                                            var buttons = new List<InlineKeyboardButton[]>
                                            {
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Название", (int)InlineMode.ChangeTitle + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("ОМС", (int)InlineMode.ChangeOMS + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Фамилия", (int)InlineMode.ChangeSurname + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Имя", (int)InlineMode.ChangeName + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Отчество", (int)InlineMode.ChangePatronomyc + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Email", (int)InlineMode.ChangeEmail + ":" + list[1])
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Дата рождения", (int)InlineMode.ChangeBirthdate + ":" + list[1])
                                                }
                                            };

                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);


                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            await botClient.SendTextMessageAsync(user.Id, "Выберите что хотите изменить:",
                                                replyMarkup: inlineKeyboard);

                                            return;
                                        }
                                    case InlineMode.ChangeTitle:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeTitle, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите название профиля");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangeOMS:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeOMS, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите номер полиса ОМС");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangeSurname:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeSurname, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите фамилию");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangeName:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeName, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите имя");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangePatronomyc:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangePatronomyc, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите отчество");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangeEmail:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeEmail, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите адрес электронной почты");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                    case InlineMode.ChangeBirthdate:
                                        {
                                            await UserService.ChangeStatement(user.Id, ProfileStatement.ChangeBirthdate, cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
                                            await botClient.SendTextMessageAsync(user.Id, "Введите дату рождения");
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            return;
                                        }
                                }
                            }


                            return;
                        }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task ClearUserMemory(Telegram.Bot.Types.User? user, CancellationToken cancellationToken)
    {
        var hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);
        if (hunter != null && await AppointmentHunterService.GetStatement(hunter.Value, cancellationToken) == HunterStatement.None)
        {
            await AppointmentHunterService.Delete(hunter.Value, cancellationToken);
        }
        await UserService.ClearCurrentHunter(user!.Id, cancellationToken);
        await UserService.ClearCurrentProfile(user!.Id, cancellationToken);
        await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);
    }

    public async static void TryToWrite(object? obj)
    {
        Console.WriteLine($"{DateTime.Now}");
        _ = Task.Run(async () =>
        {
            var cancellationToken = new CancellationToken();
            var hunters = await AppointmentHunterService.GetHuntersInProgress(cancellationToken);

            foreach (var hunter in hunters)
            {
                bool appointmentsSuccess = false;
                GetAppointments? appointments = null;
                while (!appointmentsSuccess)
                {
                    appointments = await GorzdravService.GetAppointments(hunter.LpuId, hunter.DoctorId!.Value, cancellationToken);
                    appointmentsSuccess = appointments.success;
                }

                if (appointments != null && appointments.result != null)
                {
                    List<TimetableResult> list = new();
                    if (hunter.DesiredDay != null)
                    {
                        list = appointments.result.Where(x => x.visitStart.DayOfWeek == hunter.DesiredDay).ToList();
                    }
                    else
                    {
                        list = appointments.result;
                    }

                    if (list.Count > 0)
                    {
                        TimetableResult? item = null;
                        if (hunter.DesiredTime != null)
                        {
                            item = list.LastOrDefault(x =>
                            x.visitStart.TimeOfDay <= hunter.DesiredTime.Value.TimeOfDay &&
                            x.visitEnd.TimeOfDay >= hunter.DesiredTime.Value.TimeOfDay);
                        }
                        else if (hunter.DesiredTimeFrom != null && hunter.DesiredTimeTo != null)
                        {
                            item = list.FirstOrDefault(x =>
                            x.visitStart.TimeOfDay >= hunter.DesiredTimeFrom!.Value.TimeOfDay &&
                            x.visitStart.TimeOfDay <= hunter.DesiredTimeTo!.Value.TimeOfDay);
                        }
                        else
                        {
                            item = list.FirstOrDefault();
                        }

                        if (item != null)
                        {
                            var profile = await ProfileService.GetProfileByIdAsync(hunter.PatientId, cancellationToken);
                            var user = await UserService.GetTelegramId(profile.OwnerId, cancellationToken);


                            var patientId = (await GorzdravService.GetPatient(profile.Id, hunter.LpuId, cancellationToken)).result;

                            int errorCode = 1;
                            GetPatient? patient = null;
                            while (errorCode == 1)
                            {
                                patient = await GorzdravService.GetPatient(profile.Id, hunter.LpuId, cancellationToken);
                                errorCode = patient.errorCode;
                            }

                            if (patient != null && errorCode == 0)
                            {
                                Console.WriteLine($"{item.id}");
                                bool success = false;

                                while (!success)
                                {
                                    var answer = await GorzdravService.CreateAppointment(new TelegramAppointmentBot.Context.Models.Request.CreateAnAppointment
                                    {
                                        lpuId = hunter.LpuId,
                                        patientId = patient.result,
                                        patientFirstName = profile.Name!,
                                        patientLastName = profile.Surname!,
                                        patientMiddleName = profile.Patronomyc,
                                        patientBirthdate = profile!.Birthdate!.Value,
                                        recipientEmail = profile.Email!,
                                        appointmentId = item.id,
                                        room = item.room,
                                        num = item.number,
                                        address = item.address!
                                    }, cancellationToken);
                                    success = answer.success;
                                }

                                await AppointmentHunterService.ChangeStatement(hunter.Id, HunterStatement.Finished, cancellationToken);

                                await _botClient.SendTextMessageAsync(user, "Вы записаны к врачу!");
                            }
                            else
                            {
                                await _botClient.SendTextMessageAsync(user, "Горздрав: " + patient!.message);
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"{DateTime.Now}");
        });
    }

    private static async void GorzdravError(long chatId, string message, int errorCode, CancellationToken cancellationToken)
    {
        var tryMessage = "Повторите попытку или попробуйте позже";
        var answer = $"Горздрав: {message}\n";
        if (errorCode == 1)
        {
            answer += tryMessage;
        }

        await _botClient.SendTextMessageAsync(chatId, answer);
    }
}