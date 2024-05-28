# Telegram Bot для записи к врачу СПб

[![alt](https://i.ibb.co/ssrzT7S/telegram-bot.png)](https://t.me/Medical_Appointment_SPb_Bot)
Бот позволяет отлавливать запись к врачу на желаемый день и время

## Особенности
- Удобноое создание записи к врачу
- Просмотр и отмена визитов
- Безопасное хранение данных о профилях
- Бот работает на основе горздрава 
(запись реально происходит, в этом можно убедиться через личный кабинет на официальном сайте)

[![N|Solid](https://gkbru.ru/wp-content/uploads/2022/02/zdorove.jpg?size=400x300&quality=96)](https://gorzdrav.spb.ru/)
## Возможности

- Добавление, редактирование и удаление профилей 
- Просмотр, создание и удаление записей на основе данных профиля (запись - отлавливание записи к врачу)
- Просмотр и отмена визитов профиля (визит - пойманная запись к врачу)




> Для корректной работы бота нужно
> заполнить профиль реальными данными.

> Иногда бот может столкнуться с ошибкой из-за
> особенности при взаимодействии с Горздрав API.
> В такие моменты бот постарается проинформировать
> пользователя в чём суть возникшей проблемы



## Технологии

"Запись к врачу СПб" uses a number of open source projects to work properly:

- [.NET 6] - .NET 6 is an open source platform for creating desktop, mobile and web applications that can run on any operating system
- [Visual Studio 2022] - IDE
- [EntityFrameworkCore 6] - ORM
- [Telegram.Bot] - NuGet package to interact with the Telegram API 
- [Docker] - Docker is an open platform for developing, shipping, and running applications
- [MSSQL] - MSSQL is a relational database management system (DBMS) used to store and retrieve data from other software applications

[.NET 6]: <https://dotnet.microsoft.com/download/dotnet/6.0>
[Visual Studio 2022]: <https://visualstudio.microsoft.com/vs/>
[EntityFrameworkCore 6]: https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/6.0.0
[Telegram.Bot]: https://core.telegram.org/bots/api
[Docker]: https://www.docker.com/
[MSSQL]: https://www.microsoft.com/sql-server


## Установка

Для развертывания проекта потребуется создать 2 конфигурационных фалйа (Configuration.cs)

#### Код TelegramAppointmentBot.Context\Configuration.cs
```sh
namespace TelegramAppointmentBot.Context;

public static class Configuration
{
    public static readonly string connectionString = "Server=appointmentDb,1433;Database=AppointmentBot;User Id=sa;Password=YourStrongPassword;Integrated Security=false;TrustServerCertificate=true";
}
```

#### Код TelegramAppointmentBot\Configuration.cs
```sh
namespace TelegramAppointmentBot
{
    public static class Configuration
    {
        public static readonly string botToken = "botTokenFromBotFather";
    }
}
```
Теперь можно использовать команду в папке с решением
### Linux
```sh
docker composer up
```
### Windows
```sh
docker-compose up
```

## Docker

В целях безопасности требуется заменить значение SA_PASSWORD в файле docker-compose.yml

Ввжно чтобы значение SA_PASSWORD совпадало с паролем из Context\Configuration.cs

Для базы данных используется 1433 порт. Порт для приложения определяется автоматически
## License

MIT

   
