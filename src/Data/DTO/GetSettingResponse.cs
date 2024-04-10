#nullable enable
namespace TgTranslator.Data.DTO;

public record GetSettingResponse
{
    public required string ChatTitle { get; init; }
    public string? ChatUsername { get; init; }
    public required Settings Settings { get; init; }
}