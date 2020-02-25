using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgTranslator.Services.Handlers
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(Message message);
    }
}