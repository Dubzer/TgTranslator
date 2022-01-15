using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgTranslator.Menu;

class ApplyMenu : MenuItem
{
    public ApplyMenu(string[] arguments)
    {
        Description = "Everything is done. To apply settings, click on *Apply* button and choose your chat, otherwise click on *Cancel*";
        Command = $"set:{arguments[0]}";
    }

    protected override void GenerateButtons() =>
        Buttons.Add(new List<InlineKeyboardButton>
        {
            new("✅ Apply") {SwitchInlineQuery = Command},
            new("❌ Cancel") {CallbackData = "switch " + typeof(MainMenu)}
        });
}