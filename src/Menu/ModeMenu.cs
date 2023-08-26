using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Services;

namespace TgTranslator.Menu;

public enum TranslationMode
{
    Auto,
    Forwards,
    Manual,
    LinkedChannel
}

public class ModeMenu : MenuItem
{
    public ModeMenu(IReadOnlyList<string> arguments)
    {
        ItemTitle = "Change translation mode";
        Description = "*Change translation mode*\nHere you can customize when the bot will translate messages ✨\n\n" +
                      "*Auto* — translates all messages that require it \n" +
                      "*Forwards* — translates only forwarded messages that require it \n" +
                      "*Linked channel* — translates only posts from linked channel that require it. \n" +
                      "*Manual* — translates *only* by replying on message with `@TgTranslatorBot`, `!translate` or `/tl`";

        Command = nameof(Setting.Mode).ToLowerInvariant();
    }

    protected override void GenerateButtons()
    {
        Buttons.Add(new List<InlineKeyboardButton>
        {
            new("Auto") {CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=auto"}
        });
        Buttons.Add(new List<InlineKeyboardButton>
        {
            new("Only forwards") {CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=forwards"}
        });
        Buttons.Add(new List<InlineKeyboardButton>()
        {
            new("Linked channel") {CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=linkedChannel"}
        });
        Buttons.Add(new List<InlineKeyboardButton>
        {
            new("Manual") {CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=manual"}
        });

        base.GenerateButtons();
    }
}