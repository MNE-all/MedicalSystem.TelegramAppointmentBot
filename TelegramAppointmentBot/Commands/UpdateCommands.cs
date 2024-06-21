using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Context.Models.Response;
using TelegramAppointmentBot.Models;
using TelegramAppointmentBot.Service.Contract.Interfaces;

namespace TelegramAppointmentBot.Commands
{
    public class UpdateCommands
    {
        private static IUserService UserService = null!;
        private static IAppointmentHunterService AppointmentHunterService = null!;
        private static IProfileService ProfileService = null!;
        private static IGorzdravService GorzdravService = null!;
        private static ISpecialityService SpecialityService = null!;
        private static IVisitService VisitService = null!;
        public UpdateCommands(IUserService userService, IAppointmentHunterService appointmentHunterService, IProfileService profileService, 
            IGorzdravService gorzdravService, ISpecialityService specialityService, IVisitService visitService)
        {
            UserService = userService;
            AppointmentHunterService = appointmentHunterService;
            ProfileService = profileService;
            GorzdravService = gorzdravService;
            SpecialityService = specialityService;
            VisitService = visitService;
        }

        public async void Add(User? user, ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddTitle, cancellationToken);

            await botClient.SendTextMessageAsync(
                user.Id,
                $"Введите название профиля");
        }

        public async void AddAppointment(User? user, ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var profiles = await ProfileService.GetUserProfilesAsync(user!.Id, cancellationToken);
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
        }
        public async void ShowVisits(User? user, ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var profiles = await ProfileService.GetUserProfilesAsync(user!.Id, cancellationToken);
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
        }
        public async void ShowAppointments(User? user, ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var profiles = await ProfileService.GetUserProfilesAsync(user!.Id, cancellationToken);
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
        }

        // Методы добавления отслеживания записи
        public async void AppointmentProfileId(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);
            // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");

            var getLPUs = await GorzdravService.GetLPUs(Guid.Parse(list[1]), cancellationToken);

            if (getLPUs == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if(getLPUs.success)
            {
                await UserService.ChangeCurrentProfile(user!.Id, Guid.Parse(list[1]), cancellationToken);

                var buttons = new List<InlineKeyboardButton[]>();
                foreach (var lpu in getLPUs.result)
                {
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(lpu.lpuShortName, (int)InlineMode.AppointmentLPUs + ":" + lpu.id.ToString())
                    });
                }
                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                await botClient.SendTextMessageAsync(
                    user!.Id,
                    $"Выберите медицинское учереждение:",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                GorzdravError(user!.Id, getLPUs.message!, getLPUs.errorCode, botClient, cancellationToken);
            }
        }
        public async void AppointmentLPUs(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");

            var specs = await GorzdravService.GetSpecialties(int.Parse(list[1]), cancellationToken);

            if (specs == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (specs.success)
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
                user!.Id,
                $"Выберите направление:",
                replyMarkup: inlineKeyboard);
            }
            else
            {
                GorzdravError(user!.Id, specs.message!, specs.errorCode, botClient, cancellationToken);
            }

            return;
        }
        public async void AppointmentSpecialities(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var speciality = await SpecialityService.GetBySystemId(Guid.Parse(list[1]), cancellationToken);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");

            var getDoctor = await GorzdravService.GetDoctors(speciality.lpuId, speciality.id, cancellationToken);

            if (getDoctor == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (getDoctor.success)
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
                    user!.Id,
                    $"Выберите врача:",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                GorzdravError(user!.Id, getDoctor.message!, getDoctor.errorCode, botClient, cancellationToken);
            }
        }
        public async void AppointmentDoctor(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var speciality = await SpecialityService.GetBySystemId(Guid.Parse(list[2]), cancellationToken);

            var timetable = await GorzdravService.GetTimetable(speciality.lpuId, int.Parse(list[1]), cancellationToken);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

            if (timetable == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (timetable.success)
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
                        InlineKeyboardButton.WithCallbackData($"Выбор даты",
                        $"{(int)InlineMode.AppointmentDates}:{list[1]}:{list[2]}")
                    });

                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"Любой",
                        $"{(int)InlineMode.AppointmentDay}:Любой")
                    });

                    
                    
                    

                    var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                    await botClient.SendTextMessageAsync(
                    user!.Id,
                        "Расписание врача: ",
                        replyMarkup: inlineKeyboard);
                }
            }
            else
            {
                GorzdravError(user!.Id, timetable.message!, timetable.errorCode, botClient, cancellationToken);
            }
        }
        public async void AppointmentDay(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var hunterId = await UserService.GetCurrentHunter(user!.Id!, cancellationToken);
            var hunter = await AppointmentHunterService.GetHunterById(hunterId.Value, cancellationToken);

            var timetable = await GorzdravService.GetTimetable(hunter.LpuId, hunter.DoctorId!.Value, cancellationToken);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

            System.DayOfWeek? dayOfWeek = null;
            long? dateTicks = null;

            if (int.TryParse(list[1], out var dayOfWeekInt))
            {
                dayOfWeek = (System.DayOfWeek)dayOfWeekInt;
                await AppointmentHunterService.ChangeDayOfWeek(hunterId.Value, dayOfWeek.Value, cancellationToken);
            }
            else if (DateTime.TryParse(list[1], out var date))
            {
                dateTicks = date.Ticks;
                await AppointmentHunterService.ChangeDate(hunterId.Value, date, cancellationToken);
            }
            if (timetable == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (timetable.success)
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
                    else if (dateTicks != null)
                    {
                        DateTime date = new DateTime (dateTicks.Value);
                        times = timetable.result.Where(x => x.visitEnd.ToShortDateString() == date.ToShortDateString()).ToList();
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
                                new KeyboardButton("Любое")
                            },
                        })
                    {
                        // автоматическое изменение размера клавиатуры, если не стоит true,
                        // тогда клавиатура растягивается чуть ли не до луны,
                        // проверить можете сами
                        ResizeKeyboard = true,
                    };

                    await UserService.ChangeStatement(user.Id, ProfileStatement.AddTime, cancellationToken);
                    await botClient.SendTextMessageAsync(user!.Id, answer, replyMarkup: replyKeyboard);
                }
            }
            else
            {
                GorzdravError(user!.Id, timetable.message!, timetable.errorCode, botClient, cancellationToken);
            }
        }
        public async void AppointmentDates(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var speciality = await SpecialityService.GetBySystemId(Guid.Parse(list[2]), cancellationToken);

            var timetable = await GorzdravService.GetTimetable(speciality.lpuId, int.Parse(list[1]), cancellationToken);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

            if (timetable == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (timetable.success)
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

                    foreach (var day in timetable.result)
                    {
                        buttons.Add(new[]
                        {
                                InlineKeyboardButton.WithCallbackData($"{day.visitStart.ToShortDateString()}: " +
                                new DateTime(day.visitStart.TimeOfDay.Ticks).ToString("t") +
                                " - " +
                                new DateTime(day.visitEnd.TimeOfDay.Ticks).ToString("t"),
                                $"{(int)InlineMode.AppointmentDay}:{day.visitStart.ToShortDateString()}")
                        });

                    }
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"Любой",
                        $"{(int)InlineMode.AppointmentDay}:Любой")
                    });


                    var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                    await botClient.SendTextMessageAsync(
                    user!.Id,
                        "Расписание врача: ",
                        replyMarkup: inlineKeyboard);
                }
            }
            else
            {
                GorzdravError(user!.Id, timetable.message!, timetable.errorCode, botClient, cancellationToken);
            }
        }

        // Методы просмотра и удаления визитов
        public async void VisitsShow(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);
            var profileId = Guid.Parse(list[1]);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");

            var getLpus = await GorzdravService.GetLPUs(profileId, cancellationToken);

            //TODO Можно сделать разбивку по мед. учреждениям + поиск по всем

            var buttons = new List<InlineKeyboardButton[]>();


            List<VisitResult> results = new List<VisitResult>();
            if(getLpus == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (getLpus.success)
            {

                foreach (var lpu in getLpus.result)
                {
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"{lpu.lpuShortName}",
                        $"{(int)InlineMode.LpuVisitsShow}:{profileId}:{lpu.id}")
                    });
                }

                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Все",
                    $"{(int)InlineMode.LpuVisitsShow}:{profileId}:Все")
                });

                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                await botClient.SendTextMessageAsync(user!.Id, "Выберите в каком медицинском учреждении желаете просмотреть предстоящие визиты: ",
                    replyMarkup: inlineKeyboard);
            }
            else
            {
                GorzdravError(user!.Id, getLpus.message!, getLpus.errorCode, botClient, cancellationToken);
            }
        }
        public async void LpuVisitsShow(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);

            var profileId = Guid.Parse(list[1]);
            var profile = await ProfileService.GetProfileByIdAsync(profileId, cancellationToken);

            List<VisitResult> results = new List<VisitResult>();

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Ожидание ответа от горздрава...");


            if (list[2] == "Все")
            {
                var getLpus = await GorzdravService.GetLPUs(profileId, cancellationToken);

                if (getLpus == null)
                {
                    await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                    return;
                }
                else if (getLpus.success)
                {

                    foreach (var lpu in getLpus.result)
                    {
                        var patientGet = await GorzdravService.GetPatient(profileId, lpu.id, cancellationToken);

                        if (patientGet == null)
                        {
                            await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                            return;
                        }
                        else if (!patientGet.success)
                        {
                            await botClient.SendTextMessageAsync(user!.Id, $"{lpu.lpuFullName}\n" +
                                $"{patientGet.message}");
                        }
                        else
                        {
                            var response = await GorzdravService.GetVisits(patientGet.result, lpu.id, cancellationToken);
                            if (response == null)
                            {
                                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                                return;
                            }
                            else if (response.success && response.result != null)
                            {
                                results.AddRange(response.result);
                            }
                            else
                            {
                                GorzdravError(user!.Id, response!.message!, response.errorCode, botClient, cancellationToken);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    GorzdravError(user!.Id, getLpus.message!, getLpus.errorCode, botClient, cancellationToken);
                    return;
                }
            }
            else if (int.TryParse(list[2], out var lpuId))
            {
                var patientGet = await GorzdravService.GetPatient(profileId, lpuId, cancellationToken);

                if (patientGet == null)
                {
                    await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                    return;
                }
                if (patientGet.success)
                {
                    var response = await GorzdravService.GetVisits(patientGet.result, lpuId, cancellationToken);
                    if (response == null)
                    {
                        await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                        return;
                    }
                    else if (response.success && response.result != null)
                    {
                        results.AddRange(response.result);
                    }
                    else
                    {
                        GorzdravError(user!.Id, response!.message!, response.errorCode, botClient, cancellationToken);
                        return;
                    }
                }
                else
                {
                    GorzdravError(user!.Id, patientGet.message!, patientGet.errorCode, botClient, cancellationToken);
                    return;
                }
            }

            foreach (var result in results)
            {
                var visitId = await VisitService.AddVisit(new Context.Models.Visit
                {
                    appointmentId = result.appointmentId,
                    lpuId = result.lpuId,
                    patientId = result.patientId
                }, cancellationToken);
                var button = InlineKeyboardButton.WithCallbackData("Отменить", (int)InlineMode.DeleteVisit + ":" + visitId);
                var inlineKeyboard = new InlineKeyboardMarkup(button);

                await botClient.SendTextMessageAsync(user!.Id,
                    $"Пациент: {profile.Surname} {profile.Name}\n" +
                    $"{result.lpuShortName}\n" +
                    $"{result.specialityRendingConsultation.name}\n" +
                    $"{result.visitStart.ToString("f")}\n" +
                    $"", replyMarkup: inlineKeyboard);
            }

            if (results.Count == 0)
            {
                await botClient.SendTextMessageAsync(user!.Id, "Предстоящие визиты отсутсвуют");
            }
        }
        public async void DeleteVisit(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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

            if (response == null)
            {
                await botClient.SendTextMessageAsync(user!.Id, $"Сервис горздрава временно недоступен");
                return;
            }
            else if (response.result)
            {
                await botClient.SendTextMessageAsync(user!.Id, "Запись отменена!");
            }
            else
            {
                GorzdravError(user!.Id, response.message!, response.errorCode, botClient, cancellationToken);
            }

        }


        // Методы просмотра и удаления отслеживаемых записей
        public async void HuntersShow(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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
                    dayOfWeek = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1);
                }
                else if (hunter.DesiredCurrentDay != null)
                {
                    dayOfWeek = hunter.DesiredCurrentDay.Value.ToShortDateString();
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

                

                await botClient.SendTextMessageAsync(user!.Id,
                    $"К кому: {hunter.SpecialityName}\n" +
                    $"Желаемое время: {desiredTime}\n" +
                    $"Желаемый день: {dayOfWeek}", replyMarkup: inlineKeyboard);
            }
            if (hunters.Count == 0)
            {
                await botClient.SendTextMessageAsync(user!.Id, "Запись к врачу не отслеживается");
            }
        }
        public async void DeleteHunter(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);
            await AppointmentHunterService.Delete(Guid.Parse(list[1]), cancellationToken);
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await botClient.SendTextMessageAsync(user!.Id, "Запись удалена!");
        }
        

        // Методы просмотра, удаления и редактирования профиля
        public async void ProfileInfo(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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

            await botClient.SendTextMessageAsync(user!.Id,
                    $"Название профиля: {profile.Title}\n" +
                    $"ОМС: {profile.OMS}\n" +
                    $"Фамилия: {profile.Surname}\n" +
                    $"Имя: {profile.Name}\n" +
                    $"Отчество: {profile.Patronomyc}\n" +
                    $"Дата рождения: {birthdate}\n",
                    replyMarkup: inlineKeyboard);


        }
        public async void DeleteProfile(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);
            await ProfileService.Delete(Guid.Parse(list[1]), cancellationToken);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

            await botClient.SendTextMessageAsync(user!.Id, "Профиль удалён!");
        }
        public async void ChangeProfile(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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

            await botClient.SendTextMessageAsync(user!.Id, "Выберите что хотите изменить:",
                replyMarkup: inlineKeyboard);
        }
        public async void ChangeTitle(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeTitle, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите название профиля");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangeOMS(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeOMS, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите номер полиса ОМС");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangeSurname(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeSurname, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите фамилию");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangeName(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeName, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите имя");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangePatronomyc(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangePatronomyc, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите отчество");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangeEmail(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeEmail, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите адрес электронной почты");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        public async void ChangeBirthdate(User? user, ITelegramBotClient botClient, string[] list, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await UserService.ChangeStatement(user!.Id, ProfileStatement.ChangeBirthdate, cancellationToken);
            await UserService.ChangeCurrentProfile(user.Id, Guid.Parse(list[1]), cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите дату рождения");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        // Методы-сервисы
        private static async Task ClearUserMemory(User? user, CancellationToken cancellationToken)
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
        private static async void GorzdravError(long chatId, string message, int errorCode, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var tryMessage = "Повторите попытку или попробуйте позже";
            var answer = $"Горздрав: {message}\n";
            if (errorCode == 1)
            {
                answer += tryMessage;
            }

            await botClient.SendTextMessageAsync(chatId, answer);
        }
    }
}
