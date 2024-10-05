using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TgTranslator.Services.EventHandlers;
using TgTranslator.Utils.Extensions;

namespace TgTranslator.Controllers;

[ApiController]
[Route("api/bot")]
[ServiceFilter<IpWhitelist>]
public class TelegramBotController : ControllerBase
{
    private readonly EventRouter _eventRouter;

    public TelegramBotController(EventRouter eventRouter)
    {
        _eventRouter = eventRouter;
    }

    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPost]
    public async Task<OkResult> Post([FromBody] Update update)
    {
        if (update == null)
            return Ok();

        await _eventRouter.HandleUpdate(update);
        return Ok();
    }
}
