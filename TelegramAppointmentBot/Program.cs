using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAppointmentBot;

class Program
{
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
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    // эта переменная будет содержать в себе все связанное с сообщениями
                    var message = update.Message;

                    // From - это от кого пришло сообщение (или любой другой Update)
                    var user = message.From;
                    
                    

                    // тут обрабатываем команду /start, остальные аналогично
                    if (message.Text == "/start")
                    {
                        // Тут все аналогично Inline клавиатуре, только меняются классы
                        // НО! Тут потребуется дополнительно указать один параметр, чтобы
                        // клавиатура выглядела нормально, а не как абы что

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
                        // Тут создаем нашу клавиатуру
                        var inlineKeyboard = new InlineKeyboardMarkup(
                            new
                                List<InlineKeyboardButton
                                    []>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                                {
                                    // Каждый новый массив - это дополнительные строки,
                                    // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                    new InlineKeyboardButton[] // тут создаем массив кнопок
                                    {
                                        InlineKeyboardButton.WithUrl("Это кнопка с сайтом", "https://habr.com/"),
                                        InlineKeyboardButton.WithCallbackData("А это просто кнопка", "button1"),
                                    },
                                    new InlineKeyboardButton[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Тут еще одна", "button2"),
                                        InlineKeyboardButton.WithCallbackData("И здесь", "button3"),
                                    },
                                });

                        await botClient.SendTextMessageAsync(
                            user.Id,
                            "Это inline клавиатура!",
                            replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup

                        return;
                    }

                    if (message.Text == "/reply")
                    {
                        // Тут все аналогично Inline клавиатуре, только меняются классы
                        // НО! Тут потребуется дополнительно указать один параметр, чтобы
                        // клавиатура выглядела нормально, а не как абы что

                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Привет!"),
                                    new KeyboardButton("Пока!"),
                                },
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Позвони мне!")
                                },
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Напиши моему соседу!")
                                }
                            })
                        {
                            // автоматическое изменение размера клавиатуры, если не стоит true,
                            // тогда клавиатура растягивается чуть ли не до луны,
                            // проверить можете сами
                            ResizeKeyboard = true,
                        };

                        await botClient.SendTextMessageAsync(
                            user.Id,
                            "Это reply клавиатура!",
                            replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup

                        return;
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
}