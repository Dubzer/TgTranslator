using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

    private static readonly GetSettingResponse MockGet = new()
    {
        ChatTitle = "Cute sharks",
        Settings = new Settings
        {
            TranslationMode = TranslationMode.Auto,
            Languages = ["en"],
            Delay = 0
        }
    };

    public SettingsController(TelegramBotClient botClient,
        WebAppHashService hashService,
        SettingsProcessor settingsProcessor)
    {
        _botClient = botClient;
        _hashService = hashService;
        _settingsProcessor = settingsProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> Get([Required] [FromQuery] VerifyQueryDto query)
    {
        if (query.StartParam.Contains("mock"))
            return Ok(MockGet);

        var data = await ExtractData(query);
        if (data == null)
            return BadRequest();

        var group = data.Value.group;
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

        var data = await ExtractData(query);
        if (data == null)
            return BadRequest();

        await _settingsProcessor.SetGroupConfiguration(data.Value.group.Id, request.Settings);
        return Ok();
    }

    private async Task<(ChatMember chatMember, Chat group)?> ExtractData(VerifyQueryDto query)
    {
        if (!_hashService.VerifyHash(GenerateDataString(), query.Hash))
            return null;

        if (!query.StartParam.StartsWith('i') || !long.TryParse(query.StartParam[1..], out var chatId))
            return null;

        var user = JsonSerializer.Deserialize<VerifyUserDto>(query.UserString);
        ChatMember chatMember;
        try
        {
            chatMember = await _botClient.GetChatMemberAsync(chatId, user.Id);
        }
        catch
        {
            return null;
        }

        if (chatMember.Status is not (ChatMemberStatus.Administrator or ChatMemberStatus.Creator))
            return null;

        var group = await _botClient.GetChatAsync(chatId);
        if (group.Type is not (ChatType.Supergroup or ChatType.Group))
            return null;

        return (chatMember, group);
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