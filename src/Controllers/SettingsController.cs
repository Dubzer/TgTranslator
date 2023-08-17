using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.DTO;
using TgTranslator.Services;

namespace TgTranslator.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly TelegramBotClient _botClient;
    private readonly WebAppHashService _hashService;
    private readonly SettingsProcessor _settingsProcessor;
    private readonly ILogger _logger;

    public SettingsController(TelegramBotClient botClient, WebAppHashService hashService, SettingsProcessor settingsProcessor, ILogger logger)
    {
        _botClient = botClient;
        _hashService = hashService;
        _settingsProcessor = settingsProcessor;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([Required] [FromQuery] VerifyQueryDto query)
    {
        _logger.Error("asd");
        var verificationResult = _hashService.VerifyHash(GenerateDataString(), query.Hash);
        if (verificationResult == false)
            return BadRequest();

        if(!query.StartParam.StartsWith('i') || !long.TryParse(query.StartParam[1..], out var chatId))
            return BadRequest();

        var user = JsonSerializer.Deserialize<VerifyUserDto>(query.UserString);
        ChatMember chatMember;
        try
        {
            chatMember = await _botClient.GetChatMemberAsync(chatId, user.Id);
        }
        catch
        {
            return BadRequest();
        }

        if (chatMember.Status is not (ChatMemberStatus.Administrator or ChatMemberStatus.Creator))
            return BadRequest();

        var group = await _botClient.GetChatAsync(chatId);
        if (group.Type is not (ChatType.Supergroup or ChatType.Group))
            return BadRequest();

        return Ok(new GetSettingResponse
        {
            ChatId = group.Id,
            ChatTitle = group.Title!,
            ChatUsername = group.Username,
            Settings = await _settingsProcessor.GetGroupConfiguration(group.Id)
        });
    }

    private string GenerateDataString()
    {
        var queryParams = HttpContext.Request.Query
            .Where(x => x.Key != "hash")
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}");

        return string.Join("\n", queryParams);
    }
}