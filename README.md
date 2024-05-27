# Telegram Bot для записи к врачу СПб

[![N|Solid](https://sun9-32.userapi.com/impg/CkHyiSfKgxtejKb8g3huo_Bd41gvgImQReIZgA/NOYGn-7wDkc.jpg?size=400x300&quality=96&sign=ab14a80cea531e4e141dc7ab9fbdcd27&type=album)](https://t.me/Medical_Appointment_SPb_Bot)


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



## Tech

Dillinger uses a number of open source projects to work properly:

- [.NET 6] - .NET 6 enviroment
- [Ace Editor] - awesome web-based text editor
- [markdown-it] - Markdown parser done right. Fast and easy to extend.
- [Twitter Bootstrap] - great UI boilerplate for modern web apps
- [node.js] - evented I/O for the backend
- [Express] - fast node.js network app framework [@tjholowaychuk]
- [Gulp] - the streaming build system
- [Breakdance](https://breakdance.github.io/breakdance/) - HTML
to Markdown converter
- [jQuery] - duh

And of course Dillinger itself is open source with a [public repository][dill]
 on GitHub.

## Installation

Dillinger requires [Node.js](https://nodejs.org/) v10+ to run.

Install the dependencies and devDependencies and start the server.

```sh
cd dillinger
npm i
node app
```

For production environments...

```sh
npm install --production
NODE_ENV=production node app
```

## Plugins

Dillinger is currently extended with the following plugins.
Instructions on how to use them in your own application are linked below.

| Plugin | README |
| ------ | ------ |
| Dropbox | [plugins/dropbox/README.md][PlDb] |
| GitHub | [plugins/github/README.md][PlGh] |
| Google Drive | [plugins/googledrive/README.md][PlGd] |
| OneDrive | [plugins/onedrive/README.md][PlOd] |
| Medium | [plugins/medium/README.md][PlMe] |
| Google Analytics | [plugins/googleanalytics/README.md][PlGa] |

## Development

Want to contribute? Great!

Dillinger uses Gulp + Webpack for fast developing.
Make a change in your file and instantaneously see your updates!

Open your favorite Terminal and run these commands.

First Tab:

```sh
node app
```

Second Tab:

```sh
gulp watch
```

(optional) Third:

```sh
karma test
```

#### Building for source

For production release:

```sh
gulp build --prod
```

Generating pre-built zip archives for distribution:

```sh
gulp build dist --prod
```

## Docker

Dillinger is very easy to install and deploy in a Docker container.

By default, the Docker will expose port 8080, so change this within the
Dockerfile if necessary. When ready, simply use the Dockerfile to
build the image.

```sh
cd dillinger
docker build -t <youruser>/dillinger:${package.json.version} .
```

This will create the dillinger image and pull in the necessary dependencies.
Be sure to swap out `${package.json.version}` with the actual
version of Dillinger.

Once done, run the Docker image and map the port to whatever you wish on
your host. In this example, we simply map port 8000 of the host to
port 8080 of the Docker (or whatever port was exposed in the Dockerfile):

```sh
docker run -d -p 8000:8080 --restart=always --cap-add=SYS_ADMIN --name=dillinger <youruser>/dillinger:${package.json.version}
```

> Note: `--capt-add=SYS-ADMIN` is required for PDF rendering.

Verify the deployment by navigating to your server address in
your preferred browser.

```sh
127.0.0.1:8000
```

## License

MIT

**Free Software, Hell Yeah!**

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

 
   [.NET 6]: <http://ace.ajax.org>
   [node.js]: <http://nodejs.org>
   [express]: <http://expressjs.com>
   [AngularJS]: <http://angularjs.org>
   [Gulp]: <http://gulpjs.com>
