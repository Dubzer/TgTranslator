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
using TgTranslator.Menu;
using TgTranslator.Services;

namespace TgTranslator.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly TelegramBotClient _botClient;
    private readonly WebAppHashService _hashService;
    private readonly SettingsProcessor _settingsProcessor;
    private static GetSettingResponse _mockGet = new()
    {
        ChatTitle = "Cute sharks",
        Settings = new ()
        {
            TranslationMode = TranslationMode.Auto,
            Languages = new []{"en"}
        }
    };

    public SettingsController(TelegramBotClient botClient, WebAppHashService hashService, SettingsProcessor settingsProcessor)
    {
        _botClient = botClient;
        _hashService = hashService;
        _settingsProcessor = settingsProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> Get([Required] [FromQuery] VerifyQueryDto query)
    {
        if (query.StartParam.Contains("mock"))
            return Ok(_mockGet);

        var (result, _, group) = await ExtractData(query);
        if (!result)
            return BadRequest();

        return Ok(new GetSettingResponse
        {
            ChatTitle = group.Title!,
            ChatUsername = group.Username,
            Settings = await _settingsProcessor.GetGroupConfiguration(group.Id)
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([Required] [FromQuery] VerifyQueryDto query, [FromBody] PutSettingRequest request)
    {
        if (query.StartParam.Contains("mock"))
            return Ok();

        var (result, _, group) = await ExtractData(query);
        if (!result)
            return BadRequest();

        await _settingsProcessor.SetGroupConfiguration(group.Id, request.Settings);
        return Ok();
    }

    private async Task<(bool result, ChatMember chatMember, Chat group)> ExtractData(VerifyQueryDto query)
    {
        if (!_hashService.VerifyHash(GenerateDataString(), query.Hash))
            return (false, default, default);

        if (!query.StartParam.StartsWith('i') || !long.TryParse(query.StartParam[1..], out var chatId))
            return (false, default, default);

        var user = JsonSerializer.Deserialize<VerifyUserDto>(query.UserString);
        ChatMember chatMember;
        try
        {
            chatMember = await _botClient.GetChatMemberAsync(chatId, user.Id);
        }
        catch
        {
            return (false, default, default);
        }

        if (chatMember.Status is not (ChatMemberStatus.Administrator or ChatMemberStatus.Creator))
            return (false, default, default);

        var group = await _botClient.GetChatAsync(chatId);
        if (group.Type is not (ChatType.Supergroup or ChatType.Group))
            return (false, default, default);

        return (true, chatMember, group);
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