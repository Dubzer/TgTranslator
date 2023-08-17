using System.Text.Json.Serialization;

namespace TgTranslator.Data.DTO;

public class PostSettingsRequest
{
    public long ChatId { get; set; }

    [JsonRequired]
    public Settings Settings { get; set; }
}