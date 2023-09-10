using Microsoft.AspNetCore.Mvc;

namespace TgTranslator.Data.DTO;

public class VerifyQueryDto
{
    [FromQuery(Name = "chat_instance")] public required long ChatInstance { get; set; }
    [FromQuery(Name = "chat_type")] public required string ChatType { get; set; }
    [FromQuery(Name = "start_param")] public required string StartParam { get; set; }
    [FromQuery(Name = "auth_date")] public required long AuthDate { get; set; }
    [FromQuery(Name = "hash")] public required string Hash { get; set; }
    [FromQuery(Name = "user")] public required string UserString { get; set; }
}