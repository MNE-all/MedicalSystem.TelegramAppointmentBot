using Microsoft.EntityFrameworkCore.Query.Internal;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot;
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
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            ThrowPendingUpdates = true, 
        };
        
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
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    // эта переменная будет содержать в себе все связанное с сообщениями
                    var message = update.Message;

                    // From - это от кого пришло сообщение (или любой другой Update)
                    var user = message.From;
                    
                    if (message.Text == "/start")
                    {
                        // Тут все аналогично Inline клавиатуре, только меняются классы
                        // НО! Тут потребуется дополнительно указать один параметр, чтобы
                        // клавиатура выглядела нормально, а не как абы что
                        
                        UserService.AddUser(new User
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
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

                        return;
                    }

                    if (message.Text == "Профили")
                    {
                        var profiles = await ProfileService.GetUserProfilesAsync(user.Id, cancellationToken);

                        var buttons = new List<InlineKeyboardButton[]>();
                        foreach (var profile in profiles)
                        {
                            buttons.Add(new InlineKeyboardButton[]
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

                        return;
                    }
                    await botClient.SendTextMessageAsync(
                        user.Id,
                        message.Text); // Все клавиатуры передаются в параметр replyMarkup
                    return;
                    

                } 
                case UpdateType.CallbackQuery:
                    update.Message.From.Id = update.CallbackQuery.Message.Chat.Id;
                    var pressedButtonID = update.CallbackQuery.Data; // Сюда вытягиваешь callbackData из кнопки.
                    if (pressedButtonID == "Add")
                    {
                        //TODO Функция добавления профиля
                    }
                    Console.WriteLine($"Pressed button = {pressedButtonID}"); 
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}