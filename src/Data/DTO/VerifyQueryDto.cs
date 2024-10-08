using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TgTranslator.Data.DTO;

[BindRequired]
public class VerifyQueryDto
{
    [FromQuery(Name = "chat_instance")]
    public long ChatInstance { get; init; }

    [FromQuery(Name = "chat_type")]
    public string ChatType { get; init; }

    [FromQuery(Name = "start_param")]
    public string StartParam { get; init; }

    [FromQuery(Name = "auth_date")]
    public long AuthDate { get; init; }

    [FromQuery(Name = "hash")]
    public string Hash { get; init; }

    [FromQuery(Name = "user")]
    public string UserString { get; init; }
}