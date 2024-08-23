using System.Text.Json.Serialization;

namespace TgTranslator.Models;

public class Language
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("flag")]
    public string Flag { get; set; }
}