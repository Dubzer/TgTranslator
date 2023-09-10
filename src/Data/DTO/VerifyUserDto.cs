using System.Text.Json.Serialization;

namespace TgTranslator.Data.DTO;

public class VerifyUserDto
{
    [JsonRequired]
    [JsonPropertyName("id")]
    public long Id { get; set; }
}