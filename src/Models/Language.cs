using Newtonsoft.Json;

namespace TgTranslator.Models;

public class Language
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("flag")]
    public string Flag { get; set; }
}