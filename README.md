📙 TgTranslator
===============

TgTranslator is a telegram bot that translates messages in groups. Try it for yourself: [@TgTranslatorBot](https://t.me/tgtranslatorbot)

Use it in your project support group, to study language with a tutor, and in many other ways!

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/1.png "Example")

## 3️⃣ Translation modes

This bot has 3 translation modes:

1. **Auto**. This mode automatically translates all messages that do not match the language of the main group. In addition, the message should be less than the character limit (specified in appsettins.json -> TgTranslator -> CharLimit), should not be a command (for example: /help or .help for Userbots), and the group should not be in timeout.

2. **Forwards**. Works like Auto, but translates only forwarded messages.

3. **Manual**. Translates only after replying on message with `!translate` or bot username.

## 📙 Languages
By default, bot has 105 languages supported by Google Translate.

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/language_selection.png "Languages seletion")

## ⚙️ Bot settings

In many group bots you can call menu in group chat. This one works different: the ``/settings`` command works only in private messages. It returns the main menu with Inline buttons which are the list of available settings. 

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/main_menu.png "Main Menu")


When you select an option, it offers you to choose chat where you send ready command, like `@TgTranslatorBot set:mode=auto`.

![alt text](https://raw.githubusercontent.com/Dubzer/TgTranslator/master/screenshots/apply_menu.png "Apply Menu")


This may seem a bit complicated, but it creates a lot more flexibility for larger groups. Users won't be interrupted by large message with buttons, and also, you can create prepared configurations for multiple groups.

Menu is higly customizable, so it's not hard to add new options, or even implement it in your own project.


## ▶️ Setup
You need to have Docker and PostgreSQL.

1. Clone repository and open directory:
   ```sh
   git clone https://github.com/Dubzer/TgTranslator.git && cd TgTranslator
2. Configure the `appsettings.json` (it's pretty self-explanatory)
3. Create a new class and implement `ITranslator` and `ILanguageDetector` interfaces. After that, open `DiServices.cs` and replace `TranslatePlaceholderService` with your new class.
4. Build the project.
    ```sh
    docker build -t tgtranslator.
    ```
5. Start an app container.
    ```sh
     docker run tgtranslator
    ```

## 📝 License
The project is licensed under the [MIT license](https://github.com/yet-another-devteam/SendColorBot/blob/master/LICENSE).
