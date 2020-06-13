📙 TgTranslator
===============

TgTranslator is a telegram bot that translates messages in groups. Try it for yourself: [@grouptranslator_bot](https://t.me/grouptranslator_bot)

You can see how it works in the [Fluid Navigation Gestures group](https://t.me/FluidNG_Group):

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/1.png "Example")

It also has cool configuration menu with buttons:

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/2.png "Main menu") 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/3.png "Main language setting") 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/4.png "Apply menu")

## 🛠 Tools that project uses

[.NET Core](https://dot.net) — Cross-platform general-purpose development platform.

[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) - A framework for building web apps and services with .NET and C#

[.NET Extensions](https://github.com/aspnet/Extensions) — An open-source, cross-platform set of APIs for commonly used programming patterns and utilities.

[Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) — .NET Client for Telegram Bot API.

[MongoDB](https://www.mongodb.com/) — General purpose, document-based, distributed database. Used to store groups settings.

[Yandex.Translate](https://translate.yandex.com/developers) — Universal text translation tool that uses machine translation technology developed at Yandex.

[Prometheus](https://prometheus.io/) — Used for event monitoring and alerting.

## ▶️ Build
You need .NET Core SDK 3.1+. [Download it here](https://dotnet.microsoft.com/download/dotnet-core/3.1)

You also need MongoDB to store groups settings. [Learn more](https://www.mongodb.com/)

1. Clone repository and open directory:
   ```sh
   git clone https://github.com/Dubzer/TgTranslator.git && cd TgTranslator
2. Build the project. You can also use portable mode, which doesn't require .NET runtime. [Learn more](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
    ```sh
    dotnet publish -c Release
3. Navigate to the directory that specified in the console:
   ```sh
   cd ./src/bin/Release/netcoreapp3.1/publish/
4. Insert your values in appsettings.json
   
5. Run:
    ```sh
    dotnet ./TgTranslator.dll
## 📝 License
The project is licensed under the [MIT license](https://github.com/yet-another-devteam/SendColorBot/blob/master/LICENSE).
