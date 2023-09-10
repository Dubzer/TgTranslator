#nullable enable
namespace TgTranslator.Data.DTO;

public class GetSettingResponse
{
    public required string ChatTitle { get; set; }
    public string? ChatUsername { get; set; }
    public required Settings Settings { get; set; }
}