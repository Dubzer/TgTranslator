using System;
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
    private readonly SettingsService _settingsService;
    private readonly SettingsValidator _settingsValidator;

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
        SettingsService settingsService,
        SettingsValidator settingsValidator)
    {
        _botClient = botClient;
        _hashService = hashService;
        _settingsService = settingsService;
        _settingsValidator = settingsValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] VerifyQueryDto query)
    {
        if (query.StartParam.Contains("mock"))
            return Ok(MockGet);

        if (DateTimeOffset.FromUnixTimeSeconds(query.AuthDate).UtcDateTime.AddMinutes(30) < DateTime.UtcNow)
            return BadRequest();

        var data = await ExtractData(query);
        if (data == null)
            return BadRequest();

        var group = data.Value.group;
        return Ok(new GetSettingResponse
        {
            ChatTitle = group.Title!,
            ChatUsername = group.Username,
            Settings = await _settingsService.GetSettings(group.Id)
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromQuery] VerifyQueryDto query, [FromBody] PutSettingRequest request)
    {
        if (query.StartParam.Contains("mock"))
            return Ok();

        var data = await ExtractData(query);
        if (data == null || !_settingsValidator.Validate(request.Settings).IsValid)
            return BadRequest();

        await _settingsService.SetSettings(data.Value.group.Id, request.Settings);
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