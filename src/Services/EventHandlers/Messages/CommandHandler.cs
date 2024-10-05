using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgTranslator.Exceptions;
using TgTranslator.Menu;
using TgTranslator.Utils;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Services.EventHandlers.Messages;

public class CommandHandler
{
    private readonly TelegramBotClient _client;
    private readonly BotMenu _botMenu;
    private readonly UsersDatabaseService _users;

    public CommandHandler(TelegramBotClient client, BotMenu botMenu, UsersDatabaseService users)
    {
        _client = client;
        _botMenu = botMenu;
        _users = users;
    }

    public async Task Handle(Message message)
    {
        ChatType chatType = message.Chat.Type;
        string command = message.Text![1..];
        string payload = null;

        if (command.Contains('@'))
        {
            int indexOfAt = command.IndexOf('@');
            if (command[(indexOfAt + 1)..] != Static.Username)
                return;

            command = command[..indexOfAt];
        }

        if (command.Contains(' '))
        {
            payload = command.Split(" ")[1];
            command = command.Split(" ")[0];
        }

        switch (command)
        {
            case "settings" when chatType is ChatType.Group or ChatType.Supergroup:
                if (!await message.From.IsAdministrator(message.Chat.Id, _client))
                    throw new UnauthorizedSettingChangingException();

                var bot = await _client.GetChatMemberAsync(message.Chat.Id, Static.BotId);
                if (message.From?.Id == 1087968824 && bot.Status != ChatMemberStatus.Administrator)
                {
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"⚠️ To change the settings, you need to promote @{Static.Username} to administrator status!");

                    return;
                }

                await _client.SendTextMessageAsync(message.Chat.Id,
                    "Press on the button bellow to change the settings." +
                    $"\n\nIf your client doesn't support the menu [click here](https://t.me/{Static.Username}?start=s)",
                    parseMode: ParseMode.Markdown,
                    linkPreviewOptions: TelegramUtils.DisabledLinkPreview,
                    replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Change settings")
                    {
                        Url = $"https://t.me/{Static.Username}/settings?startapp=i{message.Chat.Id}"
                    }));
                break;
            case "settings" when chatType == ChatType.Private:
                await _client.SendTextMessageAsync(message.Chat.Id,
                    "You cannot configure the bot here 😳\nPlease use this command in the group.");
                break;
            case "start" when chatType == ChatType.Private && payload == "s":
                await _botMenu.SendSettings(message.Chat.Id);
                break;
            case "start" when chatType == ChatType.Private:
                if (!string.IsNullOrEmpty(payload) && message.From != null)
                    await _users.AddFromPmIfNeeded(message.From.Id, payload);

                await _botMenu.SendStart(message.Chat.Id);
                break;
            case "contact" when chatType == ChatType.Private:
                await _client.SendTextMessageAsync(message.Chat.Id, "Developer: @Dubzer\nNews channel: @tgtrns\n\n☕️ Donate: yaso.su/feedme");
                break;
            default:
                return;
        }
    }
}