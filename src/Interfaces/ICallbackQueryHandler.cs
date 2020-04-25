using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgTranslator.Interfaces
{
    public interface ICallbackQueryHandler
    {
        Task HandleCallbackQueryAsync(CallbackQuery callbackQuery);
    }
}