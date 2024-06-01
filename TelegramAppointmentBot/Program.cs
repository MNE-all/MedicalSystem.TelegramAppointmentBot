using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAppointmentBot;
using TelegramAppointmentBot.Commands;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Service.Contract.Interfaces;
using TelegramAppointmentBot.Service.Implementation;

class Program
{
    private static IUserService UserService = new UserService();

    private static IProfileService ProfileService = new ProfileService();

    private static IGorzdravService GorzdravService = new GorzdravService();

    private static IAppointmentHunterService AppointmentHunterService = new AppointmentHunterService();

    private static IVisitService VisitService = new VisitService();

    private static ISpecialityService SpecialityService = new SpecialityService();



    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient = null!;

    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions = null!;

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
                            // Класс с ответныим действиями на введённое сообщение
                            TextCommands textCommands = new(UserService, AppointmentHunterService, ProfileService);
                            // эта переменная будет содержать в себе все связанное с сообщениями
                            var message = update.Message;

                            // From - это от кого пришло сообщение (или любой другой Update)
                            var user = message!.From;


                            switch (message.Text)
                            {
                                case "/start":
                                    textCommands.Start(user, botClient, cancellationToken);
                                    return;

                                case "Профили":
                                    textCommands.Profiles(user, botClient, cancellationToken);
                                    return;

                                case "Запись":
                                    textCommands.Record(user, botClient, cancellationToken);
                                    return;

                                default:
                                    switch (UserService.CheckStatement(user!.Id, cancellationToken).Result)
                                    {
                                        case ProfileStatement.AddTitle:
                                            textCommands.AddTitle(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddOMS:
                                            textCommands.AddOMS(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddSurname:
                                            textCommands.AddSurname(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddName:
                                            textCommands.AddName(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddPatronomyc:
                                            textCommands.AddPatronomyc(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddEmail:
                                            textCommands.AddEmail(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddBirthdate:
                                            textCommands.AddBirthdate(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.AddTime:
                                            textCommands.AddTime(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeTitle:
                                            textCommands.ChangeTitle(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeOMS:
                                            textCommands.ChangeOMS(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeSurname:
                                            textCommands.ChangeSurname(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeName:
                                            textCommands.ChangeName(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangePatronomyc:
                                            textCommands.ChangePatronomyc(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeEmail:
                                            textCommands.ChangeEmail(user, botClient, message, cancellationToken);
                                            return;
                                        case ProfileStatement.ChangeBirthdate:
                                            textCommands.ChangeBirthdate(user, botClient, message, cancellationToken);
                                            return;
                                        default:
                                            break;
                                    }
                                    return;
                            }
                        }
                    case UpdateType.CallbackQuery:
                        {
                            UpdateCommands updateCommands = new(UserService, AppointmentHunterService, ProfileService, GorzdravService, SpecialityService, VisitService);
                            // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                            var callbackQuery = update.CallbackQuery;

                            // Аналогично и с Message мы можем получить информацию о чате, о пользователе и т.д.
                            var user = callbackQuery!.From;

                            // Выводим на экран нажатие кнопки
                            Console.WriteLine($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");
;

                            if (callbackQuery.Data == "Add")
                            {
                                updateCommands.Add(user, botClient, callbackQuery, cancellationToken);
                                return;
                            }
                            else if (callbackQuery.Data == "AddAppointment")
                            {
                                updateCommands.AddAppointment(user, botClient, callbackQuery, cancellationToken);
                                return;
                            }
                            else if (callbackQuery.Data == "ShowVisits")
                            {
                                updateCommands.ShowVisits(user, botClient, callbackQuery, cancellationToken);
                                return;
                            }
                            else if (callbackQuery.Data == "ShowAppointments")
                            {
                                updateCommands.ShowAppointments(user, botClient, callbackQuery, cancellationToken);
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
                                        updateCommands.AppointmentProfileId(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.AppointmentLPUs:
                                        updateCommands.AppointmentLPUs(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.AppointmentSpecialities:
                                        updateCommands.AppointmentSpecialities(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.AppointmentDoctor:
                                        updateCommands.AppointmentDoctor(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.AppointmentDay:
                                        updateCommands.AppointmentDay(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.VisitsShow:
                                        updateCommands.VisitsShow(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.LpuVisitsShow:
                                        updateCommands.LpuVisitsShow(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.HuntersShow:
                                        updateCommands.HuntersShow(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.DeleteHunter:
                                        updateCommands.DeleteHunter(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.DeleteVisit:
                                        updateCommands.DeleteVisit(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ProfileInfo:
                                        updateCommands.ProfileInfo(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.DeleteProfile:
                                        updateCommands.DeleteProfile(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeProfile:
                                        updateCommands.ChangeProfile(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeTitle:
                                        updateCommands.ChangeTitle(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeOMS:
                                        updateCommands.ChangeOMS(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeSurname:
                                        updateCommands.ChangeSurname(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeName:
                                        updateCommands.ChangeName(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangePatronomyc:
                                        updateCommands.ChangePatronomyc(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeEmail:
                                        updateCommands.ChangeEmail(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

                                    case InlineMode.ChangeBirthdate:
                                        updateCommands.ChangeBirthdate(user, botClient, list, callbackQuery, cancellationToken);
                                        return;

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

    public static void TryToWrite(object? obj)
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
                    if (appointments != null)
                    {
                        appointmentsSuccess = appointments.success;
                    }
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


                            var patient = await GorzdravService.GetPatient(profile.Id, hunter.LpuId, cancellationToken);

                            int errorCode = 1;
                            while (errorCode == 1)
                            {
                                patient = await GorzdravService.GetPatient(profile.Id, hunter.LpuId, cancellationToken);
                                if (patient == null) return;
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
                                        patientMiddleName = profile.Patronomyc ?? "",
                                        patientBirthdate = profile!.Birthdate!.Value,
                                        recipientEmail = profile.Email!,
                                        appointmentId = item.id,
                                        room = item.room,
                                        num = item.number,
                                        address = item.address!
                                    }, cancellationToken);
                                    if (answer == null) return;

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