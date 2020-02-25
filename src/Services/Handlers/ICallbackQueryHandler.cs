using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgTranslator.Services.Handlers
{
    public interface ICallbackQueryHandler
    {
        Task HandleCallbackQueryAsync(CallbackQuery callbackQuery);
    }
}