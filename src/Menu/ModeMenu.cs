using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Services;

namespace TgTranslator.Menu
{
    public enum TranslationMode
    {
        Auto,
        Forwards,
        Manual
    }

    public class ModeMenu : MenuItem
    {
        public ModeMenu(IReadOnlyList<string> arguments)
        {
            ItemTitle = "Change translation mode";
            Description = "*Here you can change translation mode* \n\n" +
                          "*Auto* — translates all messages that require it \n" +
                          "*Forwards* — translates only forwarded messages that require it \n" +
                          "*Manual* — translates *only* by replying on message with `@grouptransalor_bot` or `!translate`";

            Command = nameof(Setting.Mode).ToLowerInvariant();
        }

        protected override void GenerateButtons()
        {
            Buttons.Add(new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton {Text = "Auto", CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=auto"}
            });
            Buttons.Add(new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton {Text = "Only forwards", CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=forwards"}
            });
            Buttons.Add(new List<InlineKeyboardButton>
            {
                new InlineKeyboardButton {Text = "Manual", CallbackData = $"switch {typeof(ApplyMenu)}#{Command}=manual"}
            });

            base.GenerateButtons();
        }
    }
}