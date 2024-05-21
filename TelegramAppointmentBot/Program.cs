using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;
using TelegramAppointmentBot.Service.Implementation;
using User = TelegramAppointmentBot.Context.Models.User;

class Program
{
    private static IUserService UserService = new UserService();

    private static IProfileService ProfileService = new ProfileService();

    private static IGorzdravService GorzdravService = new GorzdravService();

    private static IAppointmentHunterService AppointmentHunterService = new AppointmentHunterService();

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


                                var hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);
                                if (hunter != null && await AppointmentHunterService.GetStatement(hunter.Value, cancellationToken) == HunterStatement.None)
                                {
                                    await AppointmentHunterService.Delete(hunter.Value, cancellationToken);
                                }
                                await UserService.ClearCurrentHunter(user!.Id, cancellationToken);
                                await UserService.ClearCurrentProfile(user!.Id, cancellationToken);
                                await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);


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
                                hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);
                                if (await AppointmentHunterService.GetStatement(hunter.Value, cancellationToken) == HunterStatement.None)
                                {
                                    await AppointmentHunterService.Delete(hunter.Value, cancellationToken);
                                }
                                await UserService.ClearCurrentHunter(user!.Id, cancellationToken);
                                await UserService.ClearCurrentProfile(user!.Id, cancellationToken);
                                await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);

                                var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);


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

                            case "Запись":
                                hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);
                                if (hunter != null && await AppointmentHunterService.GetStatement(hunter.Value, cancellationToken) == HunterStatement.None)
                                {
                                    await AppointmentHunterService.Delete(hunter.Value, cancellationToken);
                                }
                                await UserService.ClearCurrentHunter(user!.Id, cancellationToken);
                                await UserService.ClearCurrentProfile(user!.Id, cancellationToken);
                                await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);

                                profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);


                                buttons = new List<InlineKeyboardButton[]>();
                                foreach (var profile in profiles)
                                {
                                    buttons.Add(new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData(profile.Title, InlineMode.AppointmentProfileId.ToString() + " " + profile.Id.ToString())
                                    });
                                }
                                inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                await botClient.SendTextMessageAsync(
                                    user.Id,
                                    "Выберите кого хотите записать:",
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
                                        await ProfileService.ChangeBirthdate(profileId.Value, DateTime.Parse(message.Text!), cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.Finished, cancellationToken);
                                        await ProfileService.ValidateProfile(profileId.Value, cancellationToken);
                                        profile = ProfileService.GetProfileByIdAsync(profileId.Value, cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id,
                                            $"Название профиля: {profile.Result.Title}\n" +
                                            $"ОМС: {profile.Result.OMS}\n" +
                                            $"Фамилия: {profile.Result.Surname}\n" +
                                            $"Имя: {profile.Result.Name}\n" +
                                            $"Отчество: {profile.Result.Patronomyc}\n" +
                                            $"Дата рождения: {profile.Result.Birthdate!.Value.ToString("D")}\n");
                                        break;
                                    case ProfileStatement.AddTime:
                                        hunter = await UserService.GetCurrentHunter(user.Id, cancellationToken);
                                        await AppointmentHunterService.ChangeTime(hunter.Value, DateTime.Parse(message.Text!), cancellationToken);
                                        await botClient.SendTextMessageAsync(user.Id, "Запись отслеживается");
                                        await AppointmentHunterService.ChangeStatement(hunter.Value, HunterStatement.InProgress, cancellationToken);
                                        await UserService.ChangeStatement(user!.Id, ProfileStatement.None, cancellationToken);
                                        await UserService.ClearCurrentHunter(user.Id, cancellationToken);
                                        await UserService.ClearCurrentProfile(user.Id, cancellationToken);  
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

                        if (callbackQuery.Data!.Split(" ").Length > 1)
                        {
                            var list = callbackQuery.Data!.Split(" ");
                            // Добавляем блок switch для проверки кнопок
                            switch (list[0])
                            {
                                case "AppointmentProfileId":
                                    {
                                        // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                        try
                                        {


                                            var LPUsList = await GorzdravService.GetLPUs(Guid.Parse(list[1]), cancellationToken);
                                            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);

                                            var buttons = new List<InlineKeyboardButton[]>();
                                            foreach (var lpu in LPUsList)
                                            {
                                                buttons.Add(new[]
                                                {
                                                InlineKeyboardButton.WithCallbackData(lpu.lpuFullName, InlineMode.AppointmentLPUs.ToString() + " " + lpu.id.ToString())
                                            });
                                            }
                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                $"Выберите медицинское учереждение:",
                                                replyMarkup: inlineKeyboard);
                                        }
                                        catch (Exception ex)
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                $"Сервис горздрава временно недоступен :(");
                                        }
                                        return;
                                    }

                                case "AppointmentLPUs":
                                    {
                                        var specs = await GorzdravService.GetSpecialties(int.Parse(list[1]), cancellationToken);
                                        // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                        var buttons = new List<InlineKeyboardButton[]>();
                                        foreach (var specialty in specs)
                                        {
                                            buttons.Add(new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData(specialty.name, InlineMode.AppointmentSpecialities.ToString() + " " + list[1] + " " + specialty.id.ToString())
                                            });
                                        }
                                        var inlineKeyboard = new InlineKeyboardMarkup(buttons);


                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            $"Выберите направление:",
                                            replyMarkup: inlineKeyboard);
                                        return;
                                    }
                                case "AppointmentSpecialities":
                                    {
                                        var doctors = await GorzdravService.GetDoctors(int.Parse(list[1]), int.Parse(list[2]), cancellationToken);
                                        // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                        var buttons = new List<InlineKeyboardButton[]>();
                                        foreach (var doctor in doctors)
                                        {
                                            buttons.Add(new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData(doctor.name, InlineMode.AppointmentDoctor.ToString() + " " + list[1] + " " + doctor.id.ToString())
                                            });
                                        }
                                        var inlineKeyboard = new InlineKeyboardMarkup(buttons);


                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            $"Выберите врача:",
                                            replyMarkup: inlineKeyboard);
                                        return;
                                    }
                                case "AppointmentDoctor":
                                    {
                                        var timetable = await GorzdravService.GetTimetable(int.Parse(list[1]), int.Parse(list[2]), cancellationToken);
                                        // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

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
                                                        InlineMode.AppointmentDay.ToString() + " " + list[1] + " " + list[2] + " " + day.First().visitStart.DayOfWeek.ToString())
                                                    });
                                                }
                                            }

                                            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                                            await botClient.SendTextMessageAsync(
                                            chat.Id,
                                                "Расписание врача: ",
                                                replyMarkup: inlineKeyboard);
                                        }
                                        
                                        return;
                                    }
                                case "AppointmentDay":
                                    {
                                        var timetable = await GorzdravService.GetTimetable(int.Parse(list[1]), int.Parse(list[2]), cancellationToken);
                                        var patient = await UserService.GetCurrentProfile(user.Id, cancellationToken);
                                        var appointmentId = await UserService.GetCurrentHunter(user.Id, cancellationToken);
                                        if (appointmentId != null)
                                        {
                                            var statement = await AppointmentHunterService.GetStatement(appointmentId.Value, cancellationToken);
                                            if (statement == HunterStatement.None)
                                            {
                                                await AppointmentHunterService.Delete(appointmentId.Value, cancellationToken);
                                            }
                                        }

                                        // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                        string answer = "Введите интересующее время посещения\n" +
                                            "(в рамках: ";
                                        var buttons = new List<InlineKeyboardButton[]>();
                                        if (timetable.result != null)
                                        {
                                            var times = timetable.result.Where(x => x.visitEnd.DayOfWeek.ToString() == list[3]).ToList();

                                            //
                                            var dayTime = times.First();
                                            var appointment = await AppointmentHunterService.Create(new TelegramAppointmentBot.Context.Models.Request.AppointmentHunterRequest
                                            {
                                                PatientId = patient.Value,
                                                LpuId = int.Parse(list[1]),
                                                DoctorId = int.Parse(list[2]),
                                                DesiredDay = dayTime.visitStart.DayOfWeek,
                                            }, cancellationToken);

                                            await UserService.ChangeCurrentHunter(user.Id, appointment.Id, cancellationToken);
                                            //

                                            foreach (var t in times)
                                            {
                                                answer += new DateTime(t.visitStart.TimeOfDay.Ticks).ToString("t") +
                                                        " - " +
                                                        new DateTime(t.visitEnd.TimeOfDay.Ticks).ToString("t");

                                                if (times.IndexOf(t) != times.Count - 1)
                                                {
                                                    answer += ", ";
                                                }
                                                else
                                                {
                                                    answer += "): ";
                                                }
                                            }

                                            await UserService.ChangeStatement(user.Id, ProfileStatement.AddTime, cancellationToken);
                                            await botClient.SendTextMessageAsync(chat.Id, answer);
                                        }
                                        return;
                                    }
                            }
                        }
                        

                        return;
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }


    public async static void TryToWrite(object? obj)
    {
        var cancellationToken = new CancellationToken();
        var hunters = await AppointmentHunterService.GetHuntersInProgress(cancellationToken);

        foreach (var hunter in hunters)
        {
            var appointments = await GorzdravService.GetAppointments(hunter.LpuId.Value, hunter.DoctorId.Value, cancellationToken);
            if (appointments.result != null)
            {
                var list = appointments.result.Where(x => x.visitStart.DayOfWeek == hunter.DesiredDay).ToList();
                if (list.Count > 0)
                {
                    var item = list.FirstOrDefault(x =>
                    x.visitStart.TimeOfDay <= hunter.DesiredTime.Value.TimeOfDay &&
                    x.visitEnd.TimeOfDay >= hunter.DesiredTime.Value.TimeOfDay);
                    if (item != null)
                    {
                        var profile = await ProfileService.GetProfileByIdAsync(hunter.PatientId, cancellationToken);
                        var user = await UserService.GetTelegramId(profile.OwnerId, cancellationToken);

                        var patientId = (await GorzdravService.GetPatient(profile.Id, hunter.LpuId.Value, cancellationToken)).result;
                        Console.WriteLine($"{item.id}");
                        await GorzdravService.CreateAppointment(new TelegramAppointmentBot.Context.Models.Request.CreateAnAppointment
                        {
                            lpuId = hunter.LpuId.Value,
                            patientId = patientId,
                            patientFirstName = profile.Name,
                            patientLastName = profile.Surname,
                            patientMiddleName = profile.Patronomyc,
                            patientBirthdate = profile.Birthdate.Value,
                            recipientEmail = profile.Email,
                            appointmentId = item.id,
                            room = item.room,
                            num = item.number,
                            address = item.address
                        }, cancellationToken);

                        await AppointmentHunterService.ChangeStatement(hunter.Id, HunterStatement.Finished, cancellationToken);

                        await _botClient.SendTextMessageAsync(user, "Вы записаны к врачу!");

                    }
                }
            }
        }

        Console.WriteLine($"{DateTime.Now}");

    }
}