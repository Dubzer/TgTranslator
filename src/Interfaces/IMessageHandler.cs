using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgTranslator.Interfaces;

public interface IMessageHandler
{
    Task HandleMessageAsync(Message message);
}