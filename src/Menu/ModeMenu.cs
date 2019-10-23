using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

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
            itemTitle = "Change translation mode";
            description = "*Here you can change translation mode* \n\n" +
                          "*Auto* — translates all messages that require it \n" +
                          "*Forwards* — translates only forwarded messages that require it \n" +
                          "*Manual* — translates *only* by replying on message with `@grouptransalor_bot` or `!translate`";
            
            command = "mode";
        }
        
        protected override void GenerateButtons()
        {
            buttons.Add(new List<InlineKeyboardButton> {
                new InlineKeyboardButton { Text = "Auto", CallbackData = $"switch {typeof(ApplyMenu)}#{command}=auto"}});
            buttons.Add(new List<InlineKeyboardButton> {
                new InlineKeyboardButton { Text = "Only forwards", CallbackData = $"switch {typeof(ApplyMenu)}#{command}=forwards"}});
            buttons.Add(new List<InlineKeyboardButton> {
                new InlineKeyboardButton { Text = "Manual", CallbackData = $"switch {typeof(ApplyMenu)}#{command}=manual"}});

            base.GenerateButtons();
        }
    }
}