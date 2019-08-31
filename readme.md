📙 TgTranslator
===============

TgTranslator is a telegram bot that translates messages in groups. 
You can see how it works in the [Fluid Navigation Gestures group](https://t.me/FluidNG_Group):

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/1.png "Example")

It also has cool configuration menu with buttons:

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/2.png "Main menu") 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/3.png "Main language setting") 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/4.png "Apply menu")

## 🛠 Tools that project uses

[.NET Core](https://dot.net) — Cross-platform general-purpose development platform.

[.NET Extensions](https://github.com/aspnet/Extensions) — An open-source, cross-platform set of APIs for commonly used programming patterns and utilities.

[Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) — .NET Client for Telegram Bot API.

[Redis](https://redis.io) — Used as in-memory key-value database.

[Yandex.Translate](https://translate.yandex.com/developers) — Universal text translation tool that uses machine translation technology developed at Yandex.
## ▶️ Build
You need .NET Core SDK 3.0+. [Download it here](https://dotnet.microsoft.com/download/dotnet-core/3.0)

You also need Redis database to store settings. [Learn more](https://redis.io)

1. Clone repository and open directory:
   ```sh
   git clone https://github.com/Dubzer/TgTranslator.git && cd TgTranslator
2. Build the project. You can also use portable mode, which doesn't require .NET runtime. [Learn more](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
    ```sh
    dotnet publish -c Release
3. Navigate to the directory that specified in the console:
   ```sh
   cd ./src/bin/Release/netcoreapp3.0/publish/
4. Create **configuration.json**, paste this text and insert your values:
   ```sh
    {
     "tokens": {
     "telegramapi": "Bot token. Get it at @BotFather",
     "yandex":  "YandexTranslator Api Token. Learn more: https://translate.yandex.com/developers",
     "redisip": "Redis database IP"
    }

   }
5. Run:
    ```sh
    dotnet ./TgTranslator.dll
## 📝 License
The project is licensed under the [MIT license](https://github.com/yet-another-devteam/SendColorBot/blob/master/LICENSE).
