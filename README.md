📙 TgTranslator
===============

TgTranslator is a telegram bot that translates messages in groups. Try it for yourself: [@grouptranslator_bot](https://t.me/grouptranslator_bot)

Use it in your project support group, to study language with a tutor, and in many other ways!

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/1.png "Example")

## 3️⃣ Translation modes

This bot has 3 translation modes:

1. **Auto**. This mode automatically translates all messages that do not match the language of the main group. In addition, the message should be less than the character limit (specified in appsettins.json -> TgTranslator -> CharLimit), should not be a command (for example: /help or .help for Userbots), and the group should not be in timeout.

2. **Forwards**. Works like Auto, but translates only forwarded messages.

3. **Manual**. Translates only after replying on message with `!translate` or bot username.

## 📙 Langauges
Bot has 65 languages supported by Google Translate.

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/language_selection.png "Languages seletion")

## ⚙️ Bot settings

In many group bots you can call menu in group chat. This one works different: the ``/settings`` command works only in private messages. It returns the main menu with Inline buttons which are the list of available settings. 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/main_menu.png "Main Menu")


When you select an option, it offers you to choose chat where you send ready command, like `@grouptranslator_bot set:mode=auto`.

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/apply_menu.png "Apply Menu")


This may seem a bit complicated, but it creates a lot more flexibility for larger groups. Users won't be interrupted by large message with buttons, and also, you can create prepared configurations for multiple groups.

Menu is higly customizable, so it's not hard to add new options, or even implement it in your own project.


## 🛠 Tools that project uses

[.NET Core](https://dot.net) — Cross-platform general-purpose development platform.

[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) - A framework for 

[Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) — .NET Client for Telegram Bot API.

[PostgreSQL](https://www.postgresql.org/) — Open Source relational database.

[Yandex.Translate](https://translate.yandex.com/developers) — Universal text translation tool that uses machine translation technology developed at Yandex.

[Prometheus](https://prometheus.io/) — Used for event monitoring and alerting.

## ▶️ Build
You need .NET Core SDK 3.1+. [Download it here](https://dotnet.microsoft.com/download/dotnet-core/3.1)

You also need PostgreSQL to store groups settings. [Learn more](https://www.postgresql.org/)

1. Clone repository and open directory:
   ```sh
   git clone https://github.com/Dubzer/TgTranslator.git && cd TgTranslator
2. Build the project. You can also use portable mode, which doesn't require .NET runtime. [Learn more](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
    ```sh
    dotnet publish -c Release
    ```
## 🔧 Configuration
Bot configuration is located in ``appsettings.json``

- **TgTranslator**

  ``CharLimit`` —  translation characters limit.

  ``BanTIme`` —  timeout time when admin mutes bot. Bot will not check the messages language during the timeout. (in minutes)

- **Telegram**

  ``Polling`` — defines, is bot getting updates by using polling, or webhooks.

  ``BotToken`` — token for Bot API (get in @BotFather) 

- **Yandex**

  ``TranslatorToken`` — token for Yandex.Translate API (get at https://translate.yandex.com/developers)

- **ConnectionStrings**

  ``TgTranslatorContext`` — connection string for bot's Postgres database. 

- **HelpMenu**

  ``VideoUrl`` — public URL for video that sends on ``/help`` command. Has to be accessible from the web.

 - **Kestrel**

   ``Endpoints`` — endpoints for using webhooks and metrics.

- **Prometheus**

  ``Login`` — Basic Auth login for Prometheus metrics.
  
  ``Password`` — Basic Auth password for Prometheus metrics.

- **Serilog**

  Here you can set various serilog settings.
## 📝 License
The project is licensed under the [MIT license](https://github.com/yet-another-devteam/SendColorBot/blob/master/LICENSE).
