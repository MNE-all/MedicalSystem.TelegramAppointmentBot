using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot.Context.Enums;
using TelegramAppointmentBot.Service.Contract.Interfaces;
using TelegramAppointmentBot.Service.Implementation;

namespace TelegramAppointmentBot.Commands
{
    public class TextCommands
    {
        private static IUserService UserService = null!;
        private static IAppointmentHunterService AppointmentHunterService = null!;
        private static IProfileService ProfileService = null!;
        private static IEncryptService EncryptService = new EncryptService();
        public TextCommands(IUserService userService, IAppointmentHunterService appointmentHunterService, 
            IProfileService profileService)
        {
            UserService = userService;
            AppointmentHunterService = appointmentHunterService;
            ProfileService = profileService;
        }
        
        // Методы для главных кнопок
        public async void Start(User? user, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await UserService.AddUser(new Context.Models.User
            {
                Id = user!.Id,
                FirstName = user.FirstName,
                CurrentProfile = null,
                Statement = ProfileStatement.None
            }, cancellationToken);
            await ClearUserMemory(user, cancellationToken);

            await EncryptService.Init(user!.Id);

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
        }
        public async void Profiles(User? user, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
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
        }

        // Методы для добавления записи к врачу
        public async void Record(User? user, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await ClearUserMemory(user, cancellationToken);

            var buttons = new List<InlineKeyboardButton[]>
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

            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(
                user!.Id,
                "Выберите что вам нужно:",
                replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup
        }
        public async void AddTitle(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profile = ProfileService.AddProfile(user!.Id, message.Text!, cancellationToken);
            await UserService.ChangeCurrentProfile(user!.Id, profile.Result.Id, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddOMS, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите номер полиса ОМС");
        }
        public async void AddOMS(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeOMS(profileId.Value, message.Text!, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddSurname, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите фамилию пациента");
        }
        public async void AddSurname(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeSurname(profileId.Value, message.Text!, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddName, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите имя пациента");
        }
        public async void AddName(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeName(profileId.Value, message.Text!, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddPatronomyc, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите отчество пациента");
        }
        public async void AddPatronomyc(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangePatronomyc(profileId.Value, message.Text!, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddEmail, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите email пациента");
        }
        public async void AddEmail(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeEmail(profileId.Value, message.Text!, cancellationToken);
            await UserService.ChangeStatement(user!.Id, ProfileStatement.AddBirthdate, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Введите дату рождения пациента");
        }
        public async void AddBirthdate(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            if (DateTime.TryParse(message.Text!, out var bithdate))
            {
                if (bithdate <= DateTime.Now)
                {

                    await ProfileService.ChangeBirthdate(profileId.Value, bithdate, cancellationToken);
                    await UserService.ChangeStatement(user!.Id, ProfileStatement.Finished, cancellationToken);
                    await ProfileService.ValidateProfile(profileId.Value, cancellationToken);
                    var profile = ProfileService.GetProfileByIdAsync(profileId.Value, cancellationToken);
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
                    await botClient.SendTextMessageAsync(user.Id, "Неверные данные!");
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(user.Id, "Данные в неверном виде\n" +
                    "Попробуйте ввести дату в формате дд.мм.гггг");
            }
        }

        // Метод для добавления времени отслеживаемой записи
        public async void AddTime(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var hunter = await UserService.GetCurrentHunter(user!.Id, cancellationToken);

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

            if (message.Text! == "Любое")
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
        }

        // Методы редактирования профиля
        public async void ChangeTitle(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeTitle(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Название профиля измененно!");
        }
        public async void ChangeOMS(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeOMS(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Номер полиса ОМС изменён!");
        }
        public async void ChangeSurname(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeSurname(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Фамилия изменена!");
        }
        public async void ChangeName(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeName(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Имя измененно!");
        }
        public async void ChangePatronomyc(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangePatronomyc(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Отчество измененно!");
        }
        public async void ChangeEmail(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);
            await ProfileService.ChangeEmail(profileId.Value, message.Text!, cancellationToken);
            await ClearUserMemory(user, cancellationToken);
            await botClient.SendTextMessageAsync(user.Id, "Адрес электронной почты изменён!");
        }
        public async void ChangeBirthdate(User? user, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var profileId = await UserService.GetCurrentProfile(user!.Id, cancellationToken);

            if (DateTime.TryParse(message.Text!, out var bithdate))
            {
                if (bithdate <= DateTime.Now)
                {
                    await ProfileService.ChangeBirthdate(profileId.Value, DateTime.Parse(message.Text!), cancellationToken);
                    await ClearUserMemory(user, cancellationToken);
                    await botClient.SendTextMessageAsync(user.Id, "Дата рождения изменена!");
                }
                else
                {
                    await botClient.SendTextMessageAsync(user.Id, "Неверные данные!");
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(user.Id, "Неверный тип данных");
            }
        }


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

        
    }
}
