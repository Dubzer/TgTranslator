using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgTranslator.Services;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Controllers;

[Route("api/bot")]
[ServiceFilter(typeof(IpWhitelist))]
public class TelegramBotController : Controller
{
    private readonly HandlersRouter _handlersRouter;

    public TelegramBotController(HandlersRouter handlersRouter)
    {
        _handlersRouter = handlersRouter;
    }

    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPost]
    public async Task<OkResult> Post([FromBody] Update update)
    {
        if (update == null)
            return Ok();

        await _handlersRouter.HandleUpdate(update);
        return Ok();
    }
}
